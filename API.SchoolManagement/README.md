# School Management API

This API provides endpoints for the School Management system.

## Available Endpoints

### Account Controller

#### Get Users
```
GET /api/account/users
```
Returns: List of users (limited to 10)

#### Get User by Email
```
GET /api/account/user/{email}
```
Returns: User information if found

#### Login (Web)
```
POST /api/account/login
```
Body:
```json
{
  "email": "zeca@yopmail.com",
  "password": "Admin123*",
  "rememberMe": false
}
```
Returns: Login result with user information

#### Login (Mobile)
```
POST /api/account/mobile-login
```
Body:
```json
{
  "email": "zeca@yopmail.com",
  "password": "Admin123*",
  "rememberMe": false
}
```
Returns: Login result with token and user information

#### Logout
```
POST /api/account/logout
```
Returns: Logout confirmation

#### Recover Password (Web)
```
POST /api/account/recover-password
```
Body:
```json
{
  "email": "user@email.com"
}
```
Returns: Password recovery result (sends reset link)

#### Send Verification Code (Mobile)
```
POST /api/account/send-verification-code
```
Body:
```json
{
  "email": "user@email.com"
}
```
Returns: Verification code sent to email

#### Verify Code (Mobile)
```
POST /api/account/verify-code
```
Body:
```json
{
  "email": "user@email.com",
  "code": "123456"
}
```
Returns: Code verification result

#### Reset Password with Code (Mobile)
```
POST /api/account/reset-password-with-code
```
Body:
```json
{
  "email": "user@email.com",
  "code": "123456",
  "newPassword": "NewPassword123*"
}
```
Returns: Password reset result

#### Update Password (Mobile) - Requires Authentication
```
POST /api/account/update-password
```
Headers:
```
Authorization: Bearer {token}
```
Body:
```json
{
  "newPassword": "NewPassword123*"
}
```
Returns: 
- **Success**: `{ "success": true, "message": "Password updated successfully! You can now use your new password to login.", "details": "Your password has been changed successfully. Please remember to use the new password for future logins." }`
- **Error**: `{ "success": false, "message": "Failed to update password. Please check the requirements below.", "errors": [...], "details": "Password must be at least 6 characters long. Try using a stronger password with letters, numbers, and symbols." }`

## Testing

Use Postman or any API testing tool to test these endpoints. The API runs on `https://localhost:7176`.

## Authentication

Some endpoints require authentication via Bearer token. Include the token in the Authorization header:
```
Authorization: Bearer {your_token_here}
``` 