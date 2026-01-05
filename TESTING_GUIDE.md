# API Testing Guide

## Quick Start with Swagger

The easiest way to test the API is using the built-in Swagger UI:

1. Run the application: `dotnet run`
2. Navigate to: `https://localhost:5001/swagger`
3. All endpoints are documented and testable

## Testing Authentication Flow

### 1. Register a New User

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "newteacher@example.com",
  "password": "Teacher@123",
  "role": "Teacher",
  "fullName": "John Doe",
  "age": 35,
  "gender": "Male",
  "description": "Mathematics Teacher"
}
```

**Response:** `200 OK`
```json
{
  "message": "User registered successfully. Please check your email to confirm your account.",
  "userId": "..."
}
```

**Note:** For testing, use the default accounts which have email already confirmed.

### 2. Login

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "teacher@test.com",
  "password": "Teacher@123"
}
```

**Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "...",
    "email": "teacher@test.com",
    "fullName": "Default Teacher",
    "gender": "Male",
    "age": 30,
    "description": "Default teacher account for testing",
    "roles": ["Teacher"]
  }
}
```

**Copy the token!** You'll need it for authenticated requests.

### 3. Authorize Requests

In Swagger:
1. Click the "Authorize" button (?? icon at top right)
2. Enter: `Bearer YOUR_TOKEN_HERE`
3. Click "Authorize"

In Postman:
1. Go to Authorization tab
2. Select Type: "Bearer Token"
3. Paste token in "Token" field

In cURL:
```bash
curl -H "Authorization: Bearer YOUR_TOKEN_HERE" https://localhost:5001/api/auth/me
```

## Testing Teacher Workflow

### 1. Get My Profile
```http
GET /api/auth/me
Authorization: Bearer {token}
```

### 2. Create a Classroom
```http
POST /api/classrooms
Authorization: Bearer {teacher_token}
Content-Type: application/json

{
  "title": "Mathematics 101",
  "description": "Introduction to Calculus"
}
```

**Response:** `201 Created`
```json
{
  "id": 1,
  "title": "Mathematics 101",
  "description": "Introduction to Calculus",
  "createdById": "...",
  "createdByName": "Default Teacher",
  "createdAt": "2024-01-15T10:30:00Z",
  "assignmentCount": 0,
  "studentCount": 0
}
```

### 3. Create an Assignment
```http
POST /api/classrooms/1/assignments
Authorization: Bearer {teacher_token}
Content-Type: application/json

{
  "title": "Assignment 1: Derivatives",
  "text": "Solve the following problems on derivatives...",
  "marks": 100
}
```

### 4. View Classroom Submissions
```http
GET /api/classrooms/1/submissions
Authorization: Bearer {teacher_token}
```

### 5. View Assignment Submissions
```http
GET /api/assignments/1/submissions
Authorization: Bearer {teacher_token}
```

## Testing Student Workflow

### 1. Login as Student
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "student@test.com",
  "password": "Student@123"
}
```

### 2. List All Classrooms (Public)
```http
GET /api/classrooms
```

### 3. Join a Classroom
```http
POST /api/classrooms/1/join
Authorization: Bearer {student_token}
```

**Response:** `200 OK`
```json
{
  "message": "Successfully joined the classroom"
}
```

### 4. View Classroom Details
```http
GET /api/classrooms/1
Authorization: Bearer {student_token}
```

### 5. View Assignments
```http
GET /api/classrooms/1/assignments
Authorization: Bearer {student_token}
```

### 6. Submit Assignment (File Upload)

**Using Postman:**
1. Select `POST /api/assignments/1/submission`
2. Authorization: Bearer {student_token}
3. Body ? form-data
4. Add field:
   - Key: `File` (select "File" type)
   - Value: Select a file
5. Send

**Using cURL:**
```bash
curl -X POST \
  -H "Authorization: Bearer {student_token}" \
  -F "File=@/path/to/your/file.pdf" \
  https://localhost:5001/api/assignments/1/submission
```

**Response:** `201 Created`
```json
{
  "id": 1,
  "assignmentId": 1,
  "assignmentTitle": "Assignment 1: Derivatives",
  "studentId": "...",
  "studentName": "Default Student",
  "studentEmail": "student@test.com",
  "fileUrl": "https://res.cloudinary.com/.../file.pdf",
  "submittedAt": "2024-01-15T11:00:00Z"
}
```

### 7. View My Submission
```http
GET /api/assignments/1/submission
Authorization: Bearer {student_token}
```

### 8. Get My Classrooms
```http
GET /api/classrooms/my-classrooms
Authorization: Bearer {student_token}
```

## Testing Edge Cases

### 1. Duplicate Submission (Should Fail)
```http
POST /api/assignments/1/submission
Authorization: Bearer {student_token}
```
**Expected:** `409 Conflict`
```json
{
  "message": "You have already submitted this assignment"
}
```

### 2. Unauthorized Access (Should Fail)
```http
POST /api/classrooms
Authorization: Bearer {student_token}
```
**Expected:** `403 Forbidden`

### 3. Join Already Joined Classroom (Should Fail)
```http
POST /api/classrooms/1/join
Authorization: Bearer {student_token}
```
**Expected:** `400 Bad Request`
```json
{
  "message": "You are already enrolled in this classroom"
}
```

### 4. Access Without Token (Should Fail)
```http
GET /api/auth/me
```
**Expected:** `401 Unauthorized`

### 5. Teacher Accessing Student-Only Endpoint (Should Fail)
```http
POST /api/classrooms/1/join
Authorization: Bearer {teacher_token}
```
**Expected:** `403 Forbidden`

## Password Reset Flow

### 1. Request Password Reset
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "student@test.com"
}
```

**Response:** `200 OK`
```json
{
  "message": "If the email exists, a password reset link has been sent."
}
```

### 2. Reset Password
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "userId": "...",
  "token": "...",
  "newPassword": "NewPassword@123"
}
```

## File Upload Testing

### Valid File Types
- PDF: `.pdf`
- Word: `.doc`, `.docx`
- Text: `.txt`
- Images: `.jpg`, `.jpeg`, `.png`
- Archive: `.zip`

### Test Cases

**? Valid Submission:**
- File: assignment.pdf (2MB)
- Expected: 201 Created

**? Invalid File Type:**
- File: virus.exe
- Expected: 400 Bad Request
- Message: "File type .exe is not allowed"

**? File Too Large:**
- File: large-video.mp4 (50MB)
- Expected: 400 Bad Request
- Message: "File size cannot exceed 10MB"

**? No File:**
- File: (empty)
- Expected: 400 Bad Request
- Message: "File is required"

## Common HTTP Status Codes

- **200 OK**: Request successful
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid input or validation error
- **401 Unauthorized**: Missing or invalid authentication token
- **403 Forbidden**: User doesn't have permission
- **404 Not Found**: Resource not found
- **409 Conflict**: Duplicate resource (e.g., duplicate submission)
- **500 Internal Server Error**: Server error

## Tips for Testing

1. **Use Swagger First**: Test all endpoints in Swagger before moving to Postman
2. **Save Tokens**: Keep teacher and student tokens handy
3. **Check Logs**: Monitor console output for errors
4. **Test Permissions**: Try accessing endpoints with wrong roles
5. **Test Edge Cases**: Empty inputs, special characters, max lengths
6. **File Uploads**: Test different file types and sizes
7. **Database State**: Use SQL Server Object Explorer to verify data

## Automated Testing Setup (Future)

Create a Postman collection with:
1. Environment variables for base URL and tokens
2. Pre-request scripts to login and set tokens
3. Tests to validate responses
4. Run collection with Newman for CI/CD

## Troubleshooting

### "Unauthorized" even with token
- Token might be expired (60 min)
- Re-login to get new token
- Check Bearer prefix: `Bearer {token}`

### "Forbidden" errors
- Check user role (Teacher vs Student)
- Verify endpoint authorization requirements
- Ensure user owns the resource (for updates/deletes)

### File upload fails
- Check Cloudinary configuration
- Verify file type is allowed
- Check file size < 10MB
- Review Cloudinary console for errors

### Email not sending
- Check MailSettings in appsettings
- For testing, use default confirmed accounts
- Review console logs for SMTP errors
