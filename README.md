# Assignment Management System API

A complete .NET 8 Web API for managing classrooms, assignments, and student submissions with JWT authentication.

## Features

- **Authentication & Authorization**
  - JWT-based authentication
  - Role-based access control (Teacher/Student)
  - Email verification
  - Password reset functionality

- **Classroom Management**
  - Teachers can create classrooms
  - Students can join classrooms
  - Public classroom listing

- **Assignment Management**
  - Teachers create assignments for their classrooms
  - Students view assignments in enrolled classrooms
  - CRUD operations with proper authorization

- **Submission System**
  - Students submit assignments via file upload
  - Files stored on Cloudinary
  - One submission per student per assignment
  - Teachers view all submissions for their classrooms

## Tech Stack

- .NET 8
- Entity Framework Core 8
- SQL Server Express
- ASP.NET Core Identity
- JWT Authentication
- Cloudinary (file storage)
- MailKit (email service)
- Swagger/OpenAPI

## Prerequisites

- .NET 8 SDK
- SQL Server Express (LocalDB)
- Visual Studio 2022 or VS Code

## Setup Instructions

### 1. Database Setup

Open **Package Manager Console** in Visual Studio and run:

```powershell
Add-Migration InitialCreate
Update-Database
```

This will:
- Create the database schema
- Seed the Teacher and Student roles
- Create default test accounts:
  - Teacher: `teacher@test.com` / `Teacher@123`
  - Student: `student@test.com` / `Student@123`

### 2. Configure Settings

Update `appsettings.Development.json` if needed:

- **Database**: Connection string is configured for SQL Server Express
- **JWT Secret**: Change for production use
- **Cloudinary**: Update `CloudName` with your Cloudinary account name
- **Email**: SMTP settings are pre-configured for Gmail

### 3. Run the Application

```bash
dotnet run
```

The API will be available at: `https://localhost:5001` (or the port shown in console)

Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/register` | Register new user (Teacher/Student) | Public |
| GET | `/confirm-email` | Confirm email address | Public |
| POST | `/login` | Login and get JWT token | Public |
| POST | `/forgot-password` | Request password reset | Public |
| POST | `/reset-password` | Reset password with token | Public |
| GET | `/me` | Get current user profile | Authenticated |

### Classrooms (`/api/classrooms`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/classrooms` | List all classrooms (public) | Public |
| GET | `/classrooms/{id}` | Get classroom details | Authenticated |
| POST | `/classrooms` | Create classroom | Teacher |
| POST | `/classrooms/{id}/join` | Join classroom | Student |
| GET | `/classrooms/my-classrooms` | Get user's classrooms | Authenticated |

### Assignments (`/api`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/classrooms/{classId}/assignments` | List assignments | Enrolled |
| GET | `/assignments/{id}` | Get assignment details | Enrolled |
| POST | `/classrooms/{classId}/assignments` | Create assignment | Teacher/Owner |
| PUT | `/assignments/{id}` | Update assignment | Teacher/Owner |
| DELETE | `/assignments/{id}` | Delete assignment | Teacher/Owner |

### Submissions (`/api`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/assignments/{assignmentId}/submission` | Submit assignment (file upload) | Student |
| GET | `/submissions/{id}` | Get submission details | Owner/Teacher |
| GET | `/assignments/{assignmentId}/submission` | Get student's own submission | Student |
| GET | `/classrooms/{classId}/submissions` | List all classroom submissions | Teacher/Owner |
| GET | `/assignments/{assignmentId}/submissions` | List assignment submissions | Teacher/Owner |

## Testing with Swagger

1. **Start the application** and navigate to Swagger UI
2. **Register** a new account or use default accounts:
   - Teacher: `teacher@test.com` / `Teacher@123`
   - Student: `student@test.com` / `Student@123`
3. **Login** using `/api/auth/login` to get a JWT token
4. **Authorize** in Swagger:
   - Click the "Authorize" button (??)
   - Enter: `Bearer YOUR_JWT_TOKEN`
   - Click "Authorize"
5. **Test endpoints** with proper authentication

## Sample Workflow

### Teacher Workflow
1. Login as teacher
2. Create a classroom
3. Create assignments for the classroom
4. View submissions from students

### Student Workflow
1. Login as student
2. Browse available classrooms
3. Join a classroom
4. View assignments
5. Submit assignment files

## File Upload

The submission endpoint accepts `multipart/form-data`:
- **Field name**: `File`
- **Allowed types**: PDF, DOC, DOCX, TXT, JPG, JPEG, PNG, ZIP
- **Max size**: 10MB
- Files are uploaded to Cloudinary and stored as URLs

## Security Features

- **Password Requirements**: 6+ chars, uppercase, lowercase, digit
- **Email Confirmation**: Required before login
- **JWT Expiry**: 60 minutes (configurable)
- **Role-based Authorization**: Endpoints protected by roles
- **Owner Validation**: Teachers can only modify their own content
- **Enrollment Check**: Students can only access enrolled classrooms

## Database Models

### ApplicationUser
- Extends `IdentityUser`
- Additional fields: FullName, Age, Gender, Description

### Classroom
- Created by teacher
- Many-to-many with students via StudentClassroom
- Has many assignments

### Assignment
- Belongs to classroom
- Created by teacher
- Has many submissions

### Submission
- Unique constraint: One per student per assignment
- Stores file URL from Cloudinary

## Error Handling

The API returns standard HTTP status codes:
- `200 OK`: Success
- `201 Created`: Resource created
- `400 Bad Request`: Invalid input
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Not authorized
- `404 Not Found`: Resource not found
- `409 Conflict`: Duplicate submission

## Environment Variables (Production)

For production deployment, use environment variables or Azure Key Vault:

```bash
ConnectionStrings__DefaultConnection="..."
JwtSettings__Secret="..."
Cloudinary__CloudName="..."
Cloudinary__ApiKey="..."
Cloudinary__ApiSecret="..."
MailSettings__MailPassword="..."
```

## Notes

- Default accounts have `EmailConfirmed = true` for testing
- Email confirmation links work only in development environment
- Update CORS policy for production frontend
- Change JWT secret for production
- Consider adding rate limiting for production

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server Express is running
- Verify connection string in appsettings.json
- Check Windows Authentication is enabled

### Email Not Sending
- Verify Gmail SMTP settings
- Check if "Less secure app access" is enabled (for Gmail)
- Use App Password if 2FA is enabled

### Cloudinary Upload Fails
- Verify CloudName, ApiKey, and ApiSecret
- Check file size and type restrictions
- Review Cloudinary dashboard for errors

## Future Enhancements

- [ ] Grading system for submissions
- [ ] Comments/feedback on submissions
- [ ] Assignment deadlines
- [ ] Notifications system
- [ ] Classroom invitation codes
- [ ] File download tracking
- [ ] Assignment templates
- [ ] Bulk operations

## License

This project is for educational purposes.
