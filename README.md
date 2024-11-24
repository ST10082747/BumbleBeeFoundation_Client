# **Introduction**

Welcome to the **BumbleBee Foundation Client App**! This a client facing app designed to allow members of the general public to interact with and join the mission outlined by the BumbleBee Foundation which aims to bring donors and companies to single platform that can be used to fund projects which would benefit local businesses and communities. This client app communicates with our API, which serve as the bridge between our client application and the backend database, enabling seamless interactions for various features such as donation management, user authentication, company data management, and admin data manipulation.


* * * * *

## **Project Authors**

This API was developed by **TerraTech TrailBlazers**. Below are the contributors who worked on the project:

  -   **Asanda Qwabe** -- Project Manager and Database Developer
  -   **Nkosinomusa Hadebe** -- Supporting Backend Developer
  -   **Samkelo Tshabalala** -- Backend Developer and Tester
  -   **Cameron Reese Davaniah** -- UI/Frontend Developer
  -   **Anelisiwe Sibusisiwe Ngema** -- UI/Frontend Developer
  

* * * * *
How This App Was Built
----------------------

The development of this application followed a structured process to ensure quality and alignment with project requirements. Below is an overview of the key steps taken during the app's creation:

1.  **Initial Planning**:

    -   The development team conducted meetings to define project requirements and create detailed user stories.
    -   Once finalized, these requirements and user stories were documented to guide the development process.
2.  **Database Creation**:

    -   The team's database developer created the project's database using SQL Server Management Studio (SSMS).
3.  **UI/Frontend Design**:

    -   The UI/Frontend developers worked on the application's interface designs, ensuring they met the requirements and user stories.
    -   These designs were reviewed and finalized in a team meeting before being handed over to the backend development team.
4.  **Prototype Development**:

    -   The backend development team built an initial all-in-one prototype, implementing most of the app's core functionality.
    -   This prototype was shared among team members via a .zip file in the team's work chat to ensure everyone had access to the code.
5.  **Feedback and Iteration**:

    -   Feedback was collected from the client and other stakeholders during several review sessions.
    -   The team held meetings to discuss and implement changes based on this feedback.
6.  **Splitting Functionality**:

    -   Once all the required functionality was implemented, the team separated the application into two components:
        -   **API Application**: To handle backend logic and database interactions.
        -   **Client Application**: To provide the user interface and client-side functionality.
    -   Both components were pushed to their respective GitHub repositories.
7.  **Development Workflow**:

    -   To ensure code consistency, the team adopted a local development workflow:
        -   Frontend and backend developers worked on the apps locally.
        -   Updates and new builds were shared in the team's work chat for review.
    -   The lead backend developer/tester tested all updates locally, verifying new features and fixes before committing changes to GitHub.
8.  **Streamlined Testing and Updates**:

    -   The tester provided detailed feedback on new features and updates, allowing team members to address issues locally.
    -   This streamlined pipeline ensured that only tested and approved changes were committed to the repositories, maintaining the quality of both the API and client apps.

For team member roles and responsibilities, please refer to the **Project Authors** section above.

* * * * *
* * * * *

Getting Started (Client App)
----------------------------
# This app requires an API, you can download configure the API separately, by accessing the API setup guide in this repository: [BumbleBee Foundation API app](https://github.com/ST10082747/BumbleBeeFoundation_API.git)

Follow these steps to set up and run the client application locally:

### 1\. Clone the Repository

First, clone or download the code from the **client repository**.

#### Using Git:

Open your terminal or Git Bash and run the following command:

```
git clone https://github.com/ST10082747/BumbleBeeFoundation_Client.git
```

#### Download as ZIP:

Alternatively, you can download the project as a ZIP file directly from GitHub. Click the "Code" button on the repository page, then select "Download ZIP." Once downloaded, extract the contents to a folder of your choice.

### 2\. Open the Client App in Visual Studio

-   Open **Visual Studio 2022**.
-   Select **Open a Project or Solution** from the start window.
-   Navigate to the folder where you cloned or extracted the client repository.
-   Select the solution file (`.sln`) and click **Open**.

### 3\. Configure the API URL in the Client App

To connect the client app to the API, you need to update the API URL in the `Program.cs` file:

-   In **Visual Studio**, go to the `Program.cs` file in the client app.
-   Find the following code:

```csharp
builder.Services.AddHttpClient("ApiHttpClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:5672/");
    // Add any default headers
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

```

-   Run the API app and locate the local host URL in your browser. Copy the URL in your browser and replace the URL (`https://localhost:5672/`) in the `Program.cs` file of the client app, with the URL of your running API. Ensure this matches the actual URL for your API.

> **Important:** The client app needs the exact URL for the API to connect properly. If the URL is incorrect or missing, the client won't be able to interact with the API.

### 4\. Set Up the Currency API Key

To enable the currency converter feature, you must set up an environment variable on you machine for the **currency API key**:

#### Create the Environment Variable:

-   Follow this guide: [How to create Environment Variables on Windows](https://pureinfotech.com/create-custom-environment-variables-windows-10/) to set up the environment variable on your PC.
-   Set the **variable name** to `CURRENCY_API_KEY`.
-   Set the **variable value** to this API key: `cur_live_Sh7Xkf0mTPBUJYIPZdn7TPaj3RX0KQKcmJ2UgQ1N`

#### Save and Restart:

-   After saving the environment variable, restart your PC to ensure it's properly applied.

### 5\. Run the Client App

Once everything is configured, you can run the client application:

-   Make sure the **API app** is already running. If not, you can refer to the API's README for setup instructions.
-   In **Visual Studio**, press **F5** to build and run the client application.

Both the client and API need to be running at the same time for the application to function properly. If the API isn't running, the client will be unable to connect, and you won't be able to log in or use the app features.

### Summary

1.  Clone or download the client app repository.
2.  Open the client app in Visual Studio.
3.  Update the API URL in the `Program.cs` file to match your local API.
4.  Set up the `CURRENCY_API_KEY` environment variable for currency conversion features.
5.  Run the client app in Visual Studio while the API is running.

Once both the client and API are running, you should be able to log in and interact with the app!

* * * * *

* * * * *
# **Email Features Disclaimer**

Emails and Notifications
------------------------

This application uses **AWS Simple Email Service (SES)** to handle email notifications. Below is an overview of the email features and configuration details:

### **Email Features**

1.  **Donation Notifications**:

    -   When a user makes a donation, the app sends a notification email to the organization's email address to inform the admin about the donation.
2.  **Password Reset Alerts**:

    -   When a user attempts to reset their password, the app sends an alert email to the user's email address to notify them of the request.
3.  **Document Upload Notifications**:

    -   When a user uploads documents (e.g., as part of a funding request), the app sends a notification email to the organization's email address to alert the admin.
4.  **Donation Approval Emails**:

    -   When an admin approves a donation, the app sends the user a confirmation email that includes the Section 18 certificate.
5.  **Profile Deactivation Emails**:

    -   If an admin deactivates a user's profile, the app sends an email to the user notifying them of the deactivation.

* * * * *

### **AWS SES Sandbox Environment**

This prototype is currently configured to use the **AWS SES Sandbox** environment. This means that emails are only sent to manually added and verified email addresses. For broader functionality and testing, you will need to configure your own AWS SES setup.

# Follow this guide to set up AWS SES:
[Getting Started with AWS SES](https://aws.amazon.com/ses/getting-started/)

* * * * *

### **Customizing Email Settings**

To use your own AWS SES configuration, update the `appsettings.json` file in the client application with your credentials:

```csharp
"SmtpSettings": {
   "Host": "(Your host credentials)",
   "Port": (Port number for your host),
   "UserName": "(Your AWS SES username)",
   "Password": "(Your AWS SES password)",
   "FromEmail": "(The main email address AWS SES will use to send emails)",
   "FromName": "BumbleBee App",
   "AdminEmail": "(The email address for admin notifications)"
}

```

#### **Field Descriptions:**

-   **Host**: Your SMTP host credentials provided by AWS SES.
-   **Port**: The port number for AWS SES (typically 587 or 465 for secure connections).
-   **UserName**: Your AWS SES username.
-   **Password**: Your AWS SES password.
-   **FromEmail**: The main email address that AWS SES will use to send emails to users.
-   **FromName**: The display name for outgoing emails (e.g., "BumbleBee App").
-   **AdminEmail**: The email address where admin notifications (e.g., donation or document upload alerts) will be sent.

* * * * *

### **Important Notes for Testing**

-   Verify your email addresses in the AWS SES Sandbox to ensure they can receive emails.
-   Transition out of the sandbox environment to enable sending emails to unverified addresses in production.

## **The app will still function without the email service set up, however any email addresses you provide the app will not receive notifications**


* * * * *
* * * * *

Creating an Admin User
----------------------

As the BumbleBee Foundation App prototype prepares for a production release, the development team has opted to remove the ability to register as an admin directly within the client app. This decision was made to mitigate potential security risks in a production environment. Instead, there are two methods for creating an admin account:

### 1\. **Using the Standalone Companion Console App**

For secure admin account creation, you can use our **standalone companion console app**, which you can download here: [Companion App](https://github.com/ST10082747/BumbleBeeAdminCreator.git).

-   This app generates an SQL command that a database administrator can run to manually add an admin user to the backend database.
-   Once the admin account is added, you can log in to the client app using the provided email and plaintext password.
-   For more details about the console app, refer to its documentation.

This approach minimizes security risks by avoiding direct admin creation through the client app.

* * * * *

### 2\. **Restoring the Admin Option in the Client App**

If you prefer a faster testing setup or wish to bypass the standalone app, you can re-enable the admin registration option in the client app by modifying the code:

1.  Open the client app and navigate to the `Register.cshtml` file located at: **`Views\Account\Register.cshtml`** in the Solution Explorer

2.  Locate the following section of the code:

    ```html
    <div class="form-group">
        <label asp-for="Role" class="control-label"></label>
        <select asp-for="Role" class="form-control" id="roleSelect">
            <option value="">Select a role</option>
            <option value="Company">Company</option>
            <option value="Donor">Donor</option>
            @* <option value="Admin">Admin</option> *@
        </select>
        <span asp-validation-for="Role" class="text-danger"></span>
    </div>

    ```

3.  Uncomment the following line to re-enable the admin option:

    ```
    @* <option value="Admin">Admin</option> *@

    ```

    After modification, it should look like this:

    ```
    <option value="Admin">Admin</option>

    ```

4.  Save the changes, run the client app, and navigate to the registration page. You can now select "Admin" as the role and create an admin account by entering the required information.

* * * * *

### Recommendation

For production environments, **method 1 (companion app)** is strongly recommended to maintain security best practices. Method 2 is only suggested for testing and development purposes, as enabling direct admin registration in the client app could expose vulnerabilities in a live system.
* * * * *
* * * * *

What Users Can Do in the App
----------------------------

The application provides functionality tailored to different types of users, including unauthenticated users, donors, companies, and administrators. Below is a detailed overview of the features available to each type of user:

### **Unauthenticated Users**

Users without an account can:

-   View the **Home**, **About Us**, **Contact Us**, **Want to Donate**, and **Want to Benefit** pages.
-   Navigate to the **Register** page to sign up for an account.

* * * * *

### **Donor Users**

After registering and logging in, donors are taken to the **Donor Dashboard**, where they can:

1.  **Make a Donation**:

    -   Navigate to the **Make a Donation** page to enter donation details.
    -   Proceed to the PayFast payment portal to confirm or cancel their donation.
2.  **View Donation History**:

    -   Access the **Donation History** page to view past donations.
    -   Optionally, download their donation history as a PDF report.
3.  **View Projects Requesting Funding**:

    -   Access the **Companies Seeking Funding** page to view a list of ongoing projects submitted by companies that require funding.

* * * * *

### **Company Users**

After registering and logging in, companies are taken to the **Company Dashboard**, where they can:

1.  **Request Funding**:

    -   Navigate to the **Request Funding** page to submit funding requests for projects.
    -   Fill in the form with project details and attach relevant documentation (e.g., project plans, proof of legitimacy).
    -   After submission, view the funding request details on the **Funding Request Confirmation** page, where they can also download their submitted attachments.
2.  **View Funding Request History**:

    -   Access the **Funding Request History** page to see whether requests were approved or rejected.
    -   If a request is approved, upload additional documentation to confirm the project's progress or completion.
    -   Once a project is finalized, its status will be updated to "closed" by the BumbleBee Foundation.
3.  **Make a Donation as a Company**:

    -   Navigate to the **Make a Donation** page, fill in donation details, and proceed to the PayFast payment portal.

* * * * *

### **Admin Users**

Admins are created by company employees and have access to advanced functionality. After logging in, they are taken to the **Admin Dashboard**, where they can:

1.  **Manage Users**:

    -   Access the **User Management** page to view, edit, delete, or create new users (e.g., another admin).
2.  **Manage Companies**:

    -   Access the **Company Management** page to approve or reject company applications to join the platform.
3.  **Manage Donations**:

    -   Access the **Donation Management** page to view and approve pending donations, send confirmation emails, download attachments, or view donation details.
4.  **Manage Funding Requests**:

    -   Access the **Funding Request Management** page to approve or reject requests, review attachments, or view request details.
5.  **Manage Documents**:

    -   Access the **Document Management** page to download, approve, or reject company-submitted documents (e.g., project-related files).
    -   Mark documents as received, which reflects in the company's funding request history.
    -   Close a funding request, marking the project as finalized and disabling further document uploads.
6.  **Access Reports**:

    -   Access the **Donation Report** page to view a summary of all donations and total funds received by the BumbleBee Foundation.
    -   Access the **Funding Request Report** page to view a summary of funding requests submitted by companies.

* * * * *

### **All Users**

-   All registered users can reset their password via the **Forgot Password** feature.

* * * * *
* * * * *

Testing Donations
-----------------

When making a donation, enter your details along with the card information. Below are test card details you can use:

-   **Card Number:** 4111 1111 1111 1111
-   **Expiry Month:** 12
-   **Expiry Year:** 2024
-   **CVV:** 123

> **Note:** These details are for testing purposes only and will not process real transactions.

* * * * *
* * * * *
# MVC Client GitHub Actions Workflow

## Overview

This repository includes a GitHub Actions workflow that automatically builds and analyzes the .NET 8.0 MVC client application. The workflow runs on every push to the main branch and when pull requests are created.

## Workflow Capabilities

### Build Verification

- Ensures the MVC application builds successfully

- Validates all project dependencies

- Checks Razor view compilation

- Verifies controller and model integrity

### Code Analysis

The workflow analyzes:

- Controller logic and patterns

- Model implementations

- Service layer code

- Razor view syntax and structure

- Dependency usage

- Code style and quality

### Testing

- Runs all project tests

- Validates application functionality

- Ensures changes haven't broken existing features

## Analysis Report

After each workflow run, an analysis report is generated containing:

- Build warnings and messages

- View compilation results

- Summary of checked components

- Total warning count

- Detailed analysis messages

## Accessing Results

1\. Go to the Actions tab in GitHub

2\. Select the most recent workflow run

3\. Download the "mvc-analysis-report" artifact

4\. Open the markdown file to view results

## Key Benefits

- **Zero Configuration**: Works with standard MVC projects without modifications

- **Comprehensive Analysis**: Covers all aspects of the MVC application

- **Automated Verification**: Ensures code quality on every push

- **Detailed Reporting**: Provides clear feedback on any issues

- **MVC-Specific Checks**: Includes specialized analysis for MVC components
