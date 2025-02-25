# AWS QA Course Test Project

This project contains automated tests for verifying the creation and configuration of AWS IAM policies, roles, groups, and users.

## Prerequisites

- .NET 8.0 SDK
- AWS credentials configured

## Setup

1. Clone the repository
2. Restore the dependencies: `dotnet restore`
3. Configure AWS settings in 'Config\appsettings.json'
{
    "AWS": {
        "Region": "your-aws-region"
    }
}

## Running Tests

To run the tests and generate a test report, follow these steps:

1. Build the project
2. Go to the '\CloudX Associate AWS for Testers\My project\AWS_QA_Course_Test_Project\bin\Debug\net8.0' directory
3. Open this directory in the cmd
4. Run the following command: `dotnet test AWS_QA_Course_Test_Project.dll --logger html`

## Test Report

You can fin the test report in the '\CloudX Associate AWS for Testers\My project\AWS_QA_Course_Test_Project\bin\Debug\net8.0\TestResults' directory.

## Project Structure

- `Base/`: Contains the base test class.
- `Tests/`: Contains the test classes.
- `Utils/`: Contains utility classes and methods.