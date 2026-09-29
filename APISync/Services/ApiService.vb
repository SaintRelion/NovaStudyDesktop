Imports System.Net.Http
    Imports System.Text
    Imports System.Text.Json
    Imports System.Threading.Tasks
    Imports System.Threading
    Namespace APISync.Services
        Public Class ApiService
            Private Shared ReadOnly _httpClient As New HttpClient()
            Public Shared Property BaseUrl As String = "https://smartprep_demo.srecosystem.space/"
            Shared Sub New()
                _httpClient.Timeout = TimeSpan.FromMinutes(5)
            End Sub
            Public Shared Async Function GetAsync(Of T)(endpoint As String, Optional req As Object = Nothing) As Task(Of ApiResponse(Of T))
                Try
                    Dim url = If(endpoint.StartsWith("http"), endpoint, BaseUrl & endpoint)
                    
                    ' BD AMPL KOS: Dynamic Query String Generation
                    If req IsNot Nothing Then
                        Dim props = req.GetType().GetProperties()
                        Dim queries = New List(Of String)
                        For Each p In props
                            Dim val = p.GetValue(req)
                            If val IsNot Nothing Then
                                queries.Add($"{p.Name.ToLower()}={Uri.EscapeDataString(val.ToString())}")
                            End If
                        Next
                        If queries.Count > 0 Then url &= "?" & String.Join("&", queries)
                    End If
                    Dim response = Await _httpClient.GetAsync(url)
                    Dim content = Await response.Content.ReadAsStringAsync()
                    If response.IsSuccessStatusCode Then
                        Return New ApiResponse(Of T) With {.Success = True, .Data = JsonSerializer.Deserialize(Of T)(content, New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True})}
                    Else
                        Return New ApiResponse(Of T) With {.Success = False, .ErrorMessage = content}
                    End If
                Catch ex As Exception
                    Return New ApiResponse(Of T) With {.Success = False, .ErrorMessage = ex.Message}
                End Try
            End Function
            Public Shared Async Function PostAsync(Of T)(endpoint As String, payload As Object) As Task(Of ApiResponse(Of T))
                Try
                    Dim url = If(endpoint.StartsWith("http"), endpoint, BaseUrl & endpoint)
                    Dim content = New StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                    Dim response = Await _httpClient.PostAsync(url, content)
                    Dim resText = Await response.Content.ReadAsStringAsync()
                    If response.IsSuccessStatusCode Then
                        Return New ApiResponse(Of T) With {.Success = True, .Data = JsonSerializer.Deserialize(Of T)(resText, New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True})}
                    Else
                        Return New ApiResponse(Of T) With {.Success = False, .ErrorMessage = resText}
                    End If
                Catch ex As Exception
                    Return New ApiResponse(Of T) With {.Success = False, .ErrorMessage = ex.Message}
                End Try
            End Function
            Public Shared Async Function PostRawAsync(endpoint As String, payload As Object) As Task(Of String)
                Try
                    Dim url = If(endpoint.StartsWith("http"), endpoint, BaseUrl & endpoint)
                    Dim content = New StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                    Dim response = Await _httpClient.PostAsync(url, content)
                    Dim resText = Await response.Content.ReadAsStringAsync()
                    If response.IsSuccessStatusCode Then
                        Return resText
                    Else
                        Return Nothing
                    End If
                Catch ex As Exception
                    Return Nothing
                End Try
            End Function
            Public Shared Async Function PostMultipartAsync(Of T)(endpoint As String, content As MultipartFormDataContent) As Task(Of ApiResponse(Of T))
                Try
                    Dim url = If(endpoint.StartsWith("http"), endpoint, BaseUrl & endpoint)
                    Dim response = Await _httpClient.PostAsync(url, content)
                    Dim resText = Await response.Content.ReadAsStringAsync()
                    If response.IsSuccessStatusCode Then
                        Return New ApiResponse(Of T) With {.Success = True, .Data = JsonSerializer.Deserialize(Of T)(resText, New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True})}
                    Else
                        Return New ApiResponse(Of T) With {.Success = False, .ErrorMessage = resText}
                    End If
                Catch ex As Exception
                    Return New ApiResponse(Of T) With {.Success = False, .ErrorMessage = ex.Message}
                End Try
            End Function
        End Class
    End Namespace
