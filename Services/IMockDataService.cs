using MediCamp.Models;
using MediCamp.Models.Domain;
using MediCamp.Models.ViewModels;

namespace MediCamp.Services
{
    public interface IMockDataService
    {
        // Users & Auth
        List<ApplicationUser> GetAllUsers();
        List<ApplicationUser> GetFilteredUsers(string? searchTerm, string? role, string? status);
        ApplicationUser? GetUserById(string id);
        ApplicationUser? GetUserByEmailOrNid(string identifier);
        (bool Success, string Message, ApplicationUser? User) Authenticate(string identifier, string password);
        (bool Success, string Message, ApplicationUser? User) RegisterPatient(RegisterPatientViewModel model);
        (bool Success, string Message, ApplicationUser? User) RegisterHost(RegisterHostViewModel model);
        (bool Success, string Message, ApplicationUser? User) RegisterUser(RegisterViewModel model);
        (bool Success, string Message) CreateUser(CreateUserViewModel model);
        (bool Success, string Message) UpdateUserRole(string userId, string newRole);
        (bool Success, string Message) ToggleUserStatus(string userId);
        (bool Success, string Message) ResetUserPassword(string userId, string newPassword);
        (bool Success, string Message) DeleteUser(string userId);
        (bool Success, string Message) ApproveHost(string userId);
        (bool Success, string Message) RejectHost(string userId, string rejectionReason);
        List<ApplicationUser> GetHostsByStatus(string? status);

        // System Metrics & Camps
        HomeLandingViewModel GetHomeLandingData();
        List<CampOverviewItem> GetAllCamps();
        CampOverviewItem? GetCampById(int id);
        
        // Camp Staff Requests
        List<CampStaffRequest> GetRequestsForCamp(int campId);
        List<CampStaffRequest> GetRequestsForDoctor(string doctorId);
        (bool Success, string Message) SendCampStaffRequest(int campId, string doctorId);
        (bool Success, string Message) RespondToCampStaffRequest(int requestId, string doctorId, string status);
    }
}
