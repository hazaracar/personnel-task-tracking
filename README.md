# Personnel Task Tracking System

This project was developed as part of my internship at **Kardelen Yazılım** in 2025.  
It is a role-based task and request management system built with ASP.NET Core MVC and PostgreSQL. The system is designed to help administrators, managers, and personnel manage daily tasks, requests, assignments, and internal processes efficiently.

---

## 🚀 Technologies Used

- ASP.NET Core MVC (.NET 7)
- Entity Framework Core (Code First)
- PostgreSQL (Docker-based)
- ASP.NET Identity for authentication & authorization
- SweetAlert2 for alert modals
- Chart.js for data visualization
- DinkToPdf for PDF generation
- Bootstrap 5 for UI design

---

## ✅ Key Features

- 🔐 Role-based login system (Admin / Manager / Personnel)
- 📋 Task assignment and tracking by status (Active, Completed, Canceled)
- 📝 Request management (Task Requests, Leave Requests)
- 🗂️ Organizational data management (Units, Titles, Institutions, Roles)
- 📣 Notification system and announcement board
- 📄 PDF export for task details and reports
- 🔎 Audit logging of sensitive operations (create, update, delete)
- 🔑 Password reset & forgot password support
- 📊 Dynamic dashboard views with charts per role
- 📱 Responsive layout for admin and personnel panels


## 📦 Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/hazaracar/personnel-task-tracking.git

2. Configure PostgreSQL (Docker is recommended)

3. Update appsettings.json with your DB credentials

4. Apply migrations:
   ```bash
   dotnet ef database update

5. Run the project:
   ```bash
   dotnet run


👤 Author
Hazar Acar
Computer Engineering Student @ Çukurova University

📄 License
This project was developed for educational purposes during my internship and is not intended for commercial use.

















