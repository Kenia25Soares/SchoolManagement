using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Data.Enums;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class EnrollmentRequestsController : Controller
    {
        private readonly ISubjectEnrollmentRequestRepository _enrollmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrollmentRequestsController(
            ISubjectEnrollmentRequestRepository enrollmentRepository,
            UserManager<ApplicationUser> userManager)
        {
            _enrollmentRepository = enrollmentRepository;
            _userManager = userManager;
        }

        /// <summary>
        /// Display list of enrollment requests
        /// </summary>
        /// <param name="status">Filter by status (optional)</param>
        /// <returns>View with enrollment requests</returns>
        public async Task<IActionResult> Index(EnrollmentRequestStatus? status = null)
        {
            try
            {
                List<SubjectEnrollmentRequest> requests;

                if (status.HasValue)
                {
                    requests = await _enrollmentRepository.GetRequestsByStatusAsync(status.Value);
                }
                else
                {
                    // Get all requests by combining all statuses
                    var pendingRequests = await _enrollmentRepository.GetRequestsByStatusAsync(EnrollmentRequestStatus.Pending);
                    var approvedRequests = await _enrollmentRepository.GetRequestsByStatusAsync(EnrollmentRequestStatus.Approved);
                    var rejectedRequests = await _enrollmentRepository.GetRequestsByStatusAsync(EnrollmentRequestStatus.Rejected);
                    
                    requests = pendingRequests.Concat(approvedRequests).Concat(rejectedRequests).ToList();
                }

                var viewModel = new EnrollmentRequestsIndexViewModel
                {
                    Requests = requests,
                    SelectedStatus = status,
                    PendingCount = requests.Count(r => r.Status == EnrollmentRequestStatus.Pending),
                    ApprovedCount = requests.Count(r => r.Status == EnrollmentRequestStatus.Approved),
                    RejectedCount = requests.Count(r => r.Status == EnrollmentRequestStatus.Rejected)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading enrollment requests: " + ex.Message;
                return View(new EnrollmentRequestsIndexViewModel());
            }
        }

        /// <summary>
        /// Display enrollment request details
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <returns>View with request details</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var request = await _enrollmentRepository.GetRequestWithDetailsAsync(id);
                if (request == null)
                {
                    TempData["ErrorMessage"] = "Enrollment request not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(request);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading request details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Approve enrollment request
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <returns>Redirect to index</returns>
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            return await ProcessRequest(id, EnrollmentRequestStatus.Approved);
        }

        /// <summary>
        /// Reject enrollment request
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <returns>Redirect to index</returns>
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            return await ProcessRequest(id, EnrollmentRequestStatus.Rejected);
        }

        /// <summary>
        /// Process enrollment request (approve/reject)
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="status">New status</param>
        /// <returns>Redirect to index</returns>
        private async Task<IActionResult> ProcessRequest(int id, EnrollmentRequestStatus status)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var request = await _enrollmentRepository.GetRequestWithDetailsAsync(id);
                if (request == null)
                {
                    TempData["ErrorMessage"] = "Enrollment request not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (request.Status != EnrollmentRequestStatus.Pending)
                {
                    TempData["ErrorMessage"] = "This request has already been processed.";
                    return RedirectToAction(nameof(Index));
                }

                // Update the request
                request.Status = status;
                request.ProcessedById = currentUser.Id;
                request.ProcessedDate = DateTime.UtcNow;

                await _enrollmentRepository.UpdateAsync(request);

                var statusText = status == EnrollmentRequestStatus.Approved ? "approved" : "rejected";
                TempData["SuccessMessage"] = $"Enrollment request has been {statusText} successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error processing request: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Get pending requests count for dashboard
        /// </summary>
        /// <returns>JSON with pending count</returns>
        [HttpGet]
        public async Task<IActionResult> GetPendingCount()
        {
            try
            {
                var pendingRequests = await _enrollmentRepository.GetRequestsByStatusAsync(EnrollmentRequestStatus.Pending);
                return Json(new { count = pendingRequests.Count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }
    }
}
