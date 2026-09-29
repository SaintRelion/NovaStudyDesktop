# 📚 SR-LEXICON
> **NOTICE:** All repository methods return an `ApiResponse(Of T)`. The types listed below represent the `Data` property of that response.

### 📬 ApiResponse Structure
```vb
Public Class ApiResponse(Of T)
    Public Property Success As Boolean
    Public Property Data As T
    Public Property ErrorMessage As String
End Class
```

## 📦 MODELS
### PasswordResetConfirm
- token: `String`
- new_password: `String`
### GenericResponse
- status: `String`
- message: `String`
- id: `String`
### DeleteUserRequest
- user_id: `Integer`
### DeleteResponse
- status: `String`
### UserItem
- id: `Integer`
- username: `String`
- email: `String`
- role: `String`
- status: `String`
### UserLogin
- username: `String`
- password: `String`
### AuthResponse
- status: `String`
- id: `String`
- email: `String`
- role: `String`
### UserRegister
- username: `String`
- password: `String`
- email: `String`
- role: `String`
### PasswordResetRequest
- email: `String`
### ToggleUserStatusRequest
- user_id: `Integer`
- target_status: `String`
### UpdateUserRequest
- user_id: `Integer`
- username: `String`
- email: `String`
### CategoryCreateRequest
- name: `String`
### SlotCreateRequest
- category_id: `Integer`
- slot_name: `String`
### GetByCategoryIdRequest
- category_id: `Integer`
### DeleteSlotRequest
- slot_id: `Integer`
### CategoryItem
- id: `Integer`
- name: `String`
### GetBySlotIdRequest
- slot_id: `Integer`
### QuestionnaireItem
- id: `Integer`
- questionnaire_id: `Integer`
- question_text: `String`
- choices: `Dictionary(Of String, String)`
- correct_answer: `String`
### SourceReferenceItem
- id: `Integer`
- category_id: `Integer`
- slot_name: `String`
- material_path: `String`
- questionnaire_path: `String`
- is_material_uploaded: `Boolean`
- is_questionnaire_extracted: `Boolean`
- item_count: `Integer`
- active_exam_count: `Integer`
- created_at: `String`
### SlotUpdateRequest
- slot_id: `Integer`
- new_slot_name: `String`
### UnifiedUploadRequest
- file: `Byte()`
- slot_id: `Integer`
- file_name: `String`
- file_type: `String`
### ExamDeleteRequest
- exam_id: `Integer`
### ExamDeleteResponse
- success: `Boolean`
- message: `String`
### ExamGenerationRequest
- exam_name: `String`
- total_items: `Integer`
- is_randomized: `Boolean`
- questionnaires: `Dictionary(Of String, Integer)`
### ExamGenerationResponse
- status: `String`
- message: `String`
- examination_id: `Integer`
### ExamGetRequest
- user_id: `Integer`
- exam_id: `Integer`
### ExamOut
- id: `Integer`
- exam_name: `String`
- total_items: `Integer`
- questions: `List(Of QuestionOut)`
- user_attempts: `Integer`
- topics: `List(Of String)`
### QuestionOut
- id: `Integer`
- question_text: `String`
- correct_answer: `String`
- choices: `Dictionary(Of String, String)`
- answer: `String`
### RevieweeStatusIn
- examination_id: `Integer`
### RevieweeStatusOut
- id: `Integer`
- username: `String`
- email: `String`
- has_taken: `Boolean`
### ExamRuleRequest
- examination_id: `Integer`
- per_question_timer: `Integer`
- review_timer: `Integer`
- status: `Integer`
### ExamRuleResponse
- success: `Boolean`
- message: `String`
- rule: `ExamRuleRequest`
### ExamListRequest
- user_id: `Integer`
- exam_name: `String`
### DailyExamListGroup
- exam_date: `String`
- exams: `List(Of ExamListOut)`
### ExamListOut
- id: `Integer`
- exam_name: `String`
- category_name: `String`
- created_at: `String`
- metric_count: `Integer`
### ExamRenameRequest
- exam_id: `Integer`
- new_name: `String`
### ExamRenameResponse
- success: `Boolean`
- message: `String`
- updated_name: `String`
### SubmitAnswerRequest
- answers: `List(Of AnswerIn)`
### AnswerIn
- user_id: `String`
- examination_id: `Integer`
- question_id: `Integer`
- answer_text: `String`
- correct_answer: `String`
### SubmissionSummary
- status: `String`
- message: `String`
- examination_id: `Integer`
- user_id: `String`
- score: `Integer`
- total: `Integer`
- percentage: `Double`
### GenerateAnalysisRequest
- examination_id: `Integer`
### GenerateAnalysisResponse
- success: `Boolean`
- message: `String`
- data: `AIAnalysisData`
### AIAnalysisData
- summary: `String`
- recommendations: `List(Of String)`
### ForensicAttemptRequest
- examination_id: `Integer`
- attempt_index: `Integer`
- user_id: `Integer`
### BasicAttemptResponse
- success: `Boolean`
- items: `List(Of BasicAttemptLogItem)`
### BasicAttemptLogItem
- category_id: `Integer`
- category_name: `String`
- slot_name: `String`
- question_text: `String`
- correct_answer: `String`
- student_answer: `String`
- is_correct: `Boolean`
- previous_student_answer: `String`
- previous_is_correct: `Boolean`
### ForensicAttemptResponse
- success: `Boolean`
- comparative_items: `List(Of ForensicLogItem)`
- message: `String`
### ForensicLogItem
- category_id: `Integer`
- category_name: `String`
- slot_name: `String`
- question_text: `String`
- correct_answer: `String`
- student_answer: `String`
- is_correct: `Boolean`
- previous_student_answer: `String`
- previous_is_correct: `Boolean`
- option_analysis: `Dictionary(Of String, String)`
### StatsRequest
- user_id: `Integer`
- examination_id: `Integer`
- focus: `String`
- material_ids: `List(Of Integer)`
- limit: `Integer`
### ComparativeTrendResponse
- exam_id: `Integer`
- user_id: `Integer`
- trend_label: `String`
- current_status: `String`
- delta: `Double`
- history: `List(Of BatchPerformance)`
### BatchPerformance
- attempt_number: `Integer`
- average_accuracy: `Double`
- examinee_count: `Integer`
- examinee_ids: `List(Of Integer)`
- attempt_indices: `List(Of Integer)`
- attempt_map: `Dictionary(Of Integer, Integer)`
- date_recorded: `String`
### ExamAnalyticsResponse
- overall_competency: `Double`
- topic_breakdown: `List(Of PerformanceMetric)`
- ai_analysis: `AIAnalysisData`
### PerformanceMetric
- id: `Integer`
- label: `String`
- score: `Double`
- total: `Double`
- percentage: `Double`
- slots: `List(Of SlotMetric)`
### SlotMetric
- slot_name: `String`
- score: `Double`
- total: `Double`
- percentage: `Double`
### ItemAnalysisRequest
- examination_id: `Integer`
### ItemAnalysisResponse
- examination_id: `Integer`
- items: `List(Of ItemAnalysisBatch)`
### ItemAnalysisBatch
- dateBatch: `String`
- questions: `QuestionDistribution`
- analysis: `Object`
- calculated_at: `Object`
### QuestionDistribution
- A: `Integer`
- B: `Integer`
- C: `Integer`
- D: `Integer`
- total: `Integer`
- question_text: `Object`
- correct_answer: `Object`
### GlobalExcellenceResponse
- success: `Boolean`
- subject_leaderboards: `List(Of SubjectLeaderboard)`
### SubjectLeaderboard
- topic_name: `String`
- top_performers: `List(Of LeaderEntry)`
### LeaderEntry
- rank: `Integer`
- student_name: `String`
- percentage: `Double`
- total_items: `Integer`
### GrowthTrendResponse
- trend_label: `String`
- unique_slots: `List(Of String)`
- history: `List(Of SlotHistoryPoint)`
### SlotHistoryPoint
- date_recorded: `String`
- slot_name: `String`
- accuracy: `Double`
- examinee_count: `Integer`

## 📡 REPOSITORIES
### AuthRepo
- `confirm_resetAsync`(req: `PasswordResetConfirm`) -> `GenericResponse`
- `delete_userAsync`(req: `DeleteUserRequest`) -> `DeleteResponse`
- `get_usersAsync`() -> `List(Of UserItem)`
- `loginAsync`(req: `UserLogin`) -> `AuthResponse`
- `registerAsync`(req: `UserRegister`) -> `AuthResponse`
- `request_resetAsync`(req: `PasswordResetRequest`) -> `GenericResponse`
- `toggle_statusAsync`(req: `ToggleUserStatusRequest`) -> `DeleteResponse`
- `update_userAsync`(req: `UpdateUserRequest`) -> `GenericResponse`
### SlotsRepo
- `create_categoryAsync`(req: `CategoryCreateRequest`) -> `object`
- `create_slotAsync`(req: `SlotCreateRequest`) -> `GenericResponse`
- `delete_categoryAsync`(req: `GetByCategoryIdRequest`) -> `object`
- `delete_slotAsync`(req: `DeleteSlotRequest`) -> `GenericResponse`
- `get_categoriesAsync`() -> `List(Of CategoryItem)`
- `get_items_by_slotAsync`(req: `GetBySlotIdRequest`) -> `List(Of QuestionnaireItem)`
- `get_slots_by_categoryAsync`(req: `GetByCategoryIdRequest`) -> `List(Of SourceReferenceItem)`
- `update_slot_nameAsync`(req: `SlotUpdateRequest`) -> `GenericResponse`
- `upload_source_fileAsync`(req: `UnifiedUploadRequest`) -> `GenericResponse`
### ExamRepo
- `delete_examAsync`(req: `ExamDeleteRequest`) -> `ExamDeleteResponse`
- `generate_examAsync`(req: `ExamGenerationRequest`) -> `ExamGenerationResponse`
- `get_examAsync`(req: `ExamGetRequest`) -> `ExamOut`
- `get_exam_revieweesAsync`(req: `RevieweeStatusIn`) -> `List(Of RevieweeStatusOut)`
- `get_exam_ruleAsync`(req: `ExamRuleRequest`) -> `ExamRuleResponse`
- `list_examsAsync`(req: `ExamListRequest`) -> `List(Of DailyExamListGroup)`
- `rename_examAsync`(req: `ExamRenameRequest`) -> `ExamRenameResponse`
- `submit_answersAsync`(req: `SubmitAnswerRequest`) -> `SubmissionSummary`
- `upsert_exam_ruleAsync`(req: `ExamRuleRequest`) -> `ExamRuleResponse`
### AnalyticsRepo
- `generate_overall_analysisAsync`(req: `GenerateAnalysisRequest`) -> `GenerateAnalysisResponse`
- `get_attempt_basic_comparisonAsync`(req: `ForensicAttemptRequest`) -> `BasicAttemptResponse`
- `get_attempt_forensicsAsync`(req: `ForensicAttemptRequest`) -> `ForensicAttemptResponse`
- `get_comparative_trendAsync`(req: `StatsRequest`) -> `ComparativeTrendResponse`
- `get_exam_analyticsAsync`(req: `StatsRequest`) -> `ExamAnalyticsResponse`
- `get_item_analysisAsync`(req: `ItemAnalysisRequest`) -> `ItemAnalysisResponse`
- `get_leaderboardAsync`() -> `GlobalExcellenceResponse`
- `get_slot_growth_trendAsync`(req: `StatsRequest`) -> `GrowthTrendResponse`
