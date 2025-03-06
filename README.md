# InternIntelligence_UserLogin

## Overview

**InternIntelligence_UserLogin** is an ASP.NET Core Minimal API project developed as part of the first assessment by **Intern_Intelligence**. This project is designed to provide a robust and scalable backend solution for user authentication, authorization, and role management. It leverages modern web development practices, including minimal APIs, identity management, and integration testing, to deliver a secure and efficient user management system.

## Features

### Authentication and Authorization
- **User Registration**: Allows new users to register with their details, including first name, last name, username, email, and password.
- **User Login**: Supports login functionality for both regular users and super admins, returning access and refresh tokens for secure session management.
- **Refresh Token**: Provides an endpoint to refresh the access token using the refresh token, ensuring continuous user sessions without requiring re-authentication.
- **Email Confirmation**: Implements email confirmation to verify user email addresses during the registration process.
- **Password Reset**: Enables users to request a password reset and securely update their password using a token sent to their email.

### User Management
- **User Details**: Allows users to retrieve their own details and super admins to fetch details of any user.
- **Role Assignment**: Super admins can assign roles to users, enabling role-based access control (RBAC).
- **Password Management**: Users can reset their passwords securely through a token-based mechanism.

### Role Management
- **Role Creation and Update**: Super admins can create new roles and update existing ones.
- **Role Assignment to Users**: Super admins can assign specific roles to users, facilitating fine-grained access control.
- **Role Deletion**: Super admins can delete roles, ensuring flexibility in role management.

### Additional Features
- **Email Integration**: The project supports sending emails for functionalities like email confirmation and password reset.
- **Data Annotations and Validations**: Ensures data integrity and correctness through comprehensive data annotations and validations.
- **Integration Testing**: Includes integration tests to validate the functionality and reliability of the API endpoints.

## Getting Started

### Prerequisites
- .NET 9 SDK or later
- A compatible IDE (e.g., Visual Studio, Visual Studio Code)
- SMTP server details for email functionality (if applicable)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/nurlancreus/InternIntelligence_UserLogin.git
   ```
2. Navigate to the project directory:
   ```bash
   cd InternIntelligence_UserLogin
   ```
3. Restore the dependencies:
   ```bash
   dotnet restore
   ```
4. Configure the appsettings.json file with your database connection string, SMTP server details, and other necessary configurations.

### Running the Project
1. Run the project using the following command:
   ```bash
   dotnet run
   ```
2. The API will be available at `http://localhost:5253` or `https://localhost:7254`.

### Testing
To run the integration tests, use the following command:
```bash
dotnet test
```

Certainly! Based on your project structure, I'll update the **Project Structure** section of the README file to accurately reflect your solution's organization. Here's the corrected version:

---

## Project Structure

The solution **InternIntelligence_UserLogin** is organized into multiple projects, each serving a specific purpose:

### 1. **InternIntelligence_UserLogin.API**
   - **Connected Services**: Contains services connected to the API project.
   - **Dependencies**: Manages NuGet packages and project dependencies.
   - **Properties**: Includes configuration files like `launchSettings.json`.
   - **Endpoints**: Defines the API endpoints for authentication, user management, and role management.
   - **Validators**: Contains data annotation validation logic for incoming requests.
   - **ApiConstants.cs**: Stores constants used in the API.
   - **appsettings.json**: Configuration file for application settings.
   - **Configurations.cs**: Handles application configurations.
   - **CustomExceptionHandler.cs**: Manages custom exception handling.
   - **http-client.env.json**: Environment-specific settings for HTTP client testing.
   - **InternIntelligence_UserLogin.API.http**: Contains HTTP client requests for testing endpoints.
   - **Program.cs**: Entry point of the application, where services and middleware are configured.

### 2. **InternIntelligence_UserLogin.Core**
   - **Dependencies**: Manages NuGet packages and project dependencies.
   - **Abstractions**: Contains interfaces for core business logic.
   - **DTOs**: Data Transfer Objects for transferring data between layers.
   - **Entities**: Defines the entities used in the application.
   - **Exceptions**: Custom exceptions for handling specific error scenarios.
   - **Options**: Configuration options for the application.
   - **ValidationAttributes**: Custom validation attributes for data annotations.

### 3. **InternIntelligence_UserLogin.Infrastructure**
   - **Dependencies**: Manages NuGet packages and project dependencies.
   - **Persistence**: Handles database context, data seeding, migrations and services for users, roles and auth.
   - **Services**: Contains service implementations for token and mail.
   - **Helpers.cs**: Utility and helper methods used across the infrastructure layer.

---

This structure ensures a clean separation of concerns, with the **API** project handling the presentation layer, the **Core** project managing the business logic, and the **Infrastructure** project dealing with data access and external services.

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request with your changes. Ensure that your code follows the project's coding standards and includes appropriate tests.

## Acknowledgments
- **Intern_Intelligence** for providing the opportunity to work on this project.

---

For any questions or issues, please open an issue on the GitHub repository or contact the project maintainers.
