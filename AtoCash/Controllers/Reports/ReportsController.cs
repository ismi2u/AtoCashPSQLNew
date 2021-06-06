using AtoCash.Authentication;
using AtoCash.Data;
using AtoCash.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using LinqKit;
using System.Data.Entity;

namespace AtoCash.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User, AccPayable")]
    public class ReportsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public ReportsController(AtoCashDbContext context, IWebHostEnvironment hostEnv, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            //var user = System.Threading.Thread.CurrentPrincipal;
            //var TheUser = User.Identity.IsAuthenticated ? UserRepository.GetUser(user.Identity.Name) : null;
            _context = context;
            hostingEnvironment = hostEnv;
            this.roleManager = roleManager;
            this.userManager = userManager;
            //Get Logged in User's EmpId.
            //var   LoggedInEmpid = User.Identities.First().Claims.ToList().Where(x => x.Type == "EmployeeId").Select(c => c.Value);
        }



        [HttpPost]
        [ActionName("GetUsersByRoleIdReport")]
        public async Task<IActionResult> GetUsersByRoleIdReport(RoleToUserSearch searchmodel)
        {
            List<UserByRole> ListUserByRole = new();

            if (!string.IsNullOrEmpty(searchmodel.RoleId))
            {
                string rolName = roleManager.Roles.Where(r => r.Id == searchmodel.RoleId).FirstOrDefault().Name;

                //  List<string> UserIds =  context.UserRoles.Where(r => r.RoleId == id).Select(b => b.UserId).Distinct().ToList();

                var AllRoles = roleManager.Roles.ToList();

                var usersOfRole = await userManager.GetUsersInRoleAsync(rolName);

                foreach (ApplicationUser user in usersOfRole)
                {
                    UserByRole userByRole = new();

                    var emp = await _context.Employees.FindAsync(user.EmployeeId);
                    userByRole.UserId = user.Id;
                    userByRole.Id = emp.Id;
                    userByRole.UserFullName = emp.GetFullName();
                    userByRole.Email = emp.Email;
                    userByRole.EmpCode = emp.EmpCode;
                    userByRole.DOB = emp.DOB;
                    userByRole.DOJ = emp.DOJ;
                    userByRole.Gender = emp.Gender;
                    userByRole.MobileNumber = emp.MobileNumber;
                    userByRole.Department = _context.Departments.Find(emp.DepartmentId).DeptCode + ":" + _context.Departments.Find(emp.DepartmentId).DeptName;
                    userByRole.JobRole = _context.JobRoles.Find(emp.RoleId).RoleCode + ":" + _context.JobRoles.Find(emp.RoleId).RoleName;
                    userByRole.AccessRole = rolName;
                    userByRole.StatusType = _context.StatusTypes.Find(emp.StatusTypeId).Status;

                    ListUserByRole.Add(userByRole);
                }
            }
            else
            {

                var users = userManager.Users.ToList();
                foreach (var user in users)
                {
                    UserByRole userByRole = new();
                    var roles = await userManager.GetRolesAsync(user);


                    if (user.EmployeeId == 0)
                    {
                        continue;
                    }
                    var emp = await _context.Employees.FindAsync(user.EmployeeId);

                    userByRole.UserId = user.Id;
                    userByRole.Id = emp.Id;
                    userByRole.UserFullName = emp.GetFullName();
                    userByRole.Email = emp.Email;
                    userByRole.EmpCode = emp.EmpCode;
                    userByRole.DOB = emp.DOB;
                    userByRole.DOJ = emp.DOJ;
                    userByRole.Gender = emp.Gender;
                    userByRole.MobileNumber = emp.MobileNumber;
                    userByRole.Department = _context.Departments.Find(emp.DepartmentId).DeptCode + ":" + _context.Departments.Find(emp.DepartmentId).DeptName;
                    userByRole.JobRole = _context.JobRoles.Find(emp.RoleId).RoleCode + ":" + _context.JobRoles.Find(emp.RoleId).RoleName;
                    userByRole.StatusType = _context.StatusTypes.Find(emp.StatusTypeId).Status;
                    foreach (var r in roles)
                    {

                        if (userByRole.AccessRole == null)
                        {
                            userByRole.AccessRole = "";
                        }
                        if (userByRole.AccessRole == "")
                        {
                            userByRole.AccessRole = r;
                        }
                        else
                        {
                            userByRole.AccessRole = userByRole.AccessRole + "," + r;
                        }
                    }
                    ListUserByRole.Add(userByRole);
                }
            }


            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[13]
                {
                    //new DataColumn("Id", typeof(int)),
                    new DataColumn("UserId", typeof(string)),
                    new DataColumn("EmployeeId", typeof(int)),
                    new DataColumn("UserFullName", typeof(string)),
                    new DataColumn("Email",typeof(string)),
                    new DataColumn("EmpCode",typeof(string)),
                    new DataColumn("DOB",typeof(DateTime)),
                    new DataColumn("DOJ", typeof(DateTime)),
                    new DataColumn("Gender",typeof(string)),
                    new DataColumn("MobileNumber",typeof(string)),
                    new DataColumn("Department",typeof(string)),
                    new DataColumn("JobRole", typeof(string)),
                    new DataColumn("StatusType", typeof(string)),
                    new DataColumn("AccessRole", typeof(string))

                });

            foreach (var usr in ListUserByRole)
            {
                dt.Rows.Add(
                    usr.UserId,
                    usr.Id,
                    usr.UserFullName,
                    usr.Email,
                    usr.EmpCode,
                    usr.DOB,
                    usr.DOJ,
                    usr.Gender,
                    usr.MobileNumber,
                    usr.Department,
                    usr.JobRole,
                    usr.StatusType,
                    usr.AccessRole
                    );
            }
            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook
            List<string> docUrls = new();
            var docUrl = GetExcel("GetUsersByRoleId", dt);

            docUrls.Add(docUrl);

            return Ok(docUrls);
        }

        [HttpPost]
        [ActionName("GetUsersByRoleId")]
        public async Task<IActionResult> GetUsersByRoleId(RoleToUserSearch searchmodel)
        {
            List<UserByRole> ListUserByRole = new();

            if (!string.IsNullOrEmpty(searchmodel.RoleId))
            {
                string rolName = roleManager.Roles.Where(r => r.Id == searchmodel.RoleId).FirstOrDefault().Name;

                //  List<string> UserIds =  context.UserRoles.Where(r => r.RoleId == id).Select(b => b.UserId).Distinct().ToList();

                var AllRoles = roleManager.Roles.ToList();

                var usersOfRole = await userManager.GetUsersInRoleAsync(rolName);

                foreach (ApplicationUser user in usersOfRole)
                {
                    UserByRole userByRole = new();

                    var emp = await _context.Employees.FindAsync(user.EmployeeId);
                    userByRole.UserId = user.Id;
                    userByRole.Id = emp.Id;
                    userByRole.UserFullName = emp.GetFullName();
                    userByRole.Email = emp.Email;
                    userByRole.EmpCode = emp.EmpCode;
                    userByRole.DOB = emp.DOB;
                    userByRole.DOJ = emp.DOJ;
                    userByRole.Gender = emp.Gender;
                    userByRole.MobileNumber = emp.MobileNumber;
                    userByRole.Department = _context.Departments.Find(emp.DepartmentId).DeptCode + ":" + _context.Departments.Find(emp.DepartmentId).DeptName;
                    userByRole.JobRole = _context.JobRoles.Find(emp.RoleId).RoleCode + ":" + _context.JobRoles.Find(emp.RoleId).RoleName;
                    userByRole.AccessRole = rolName;
                    userByRole.StatusType = _context.StatusTypes.Find(emp.StatusTypeId).Status;

                    ListUserByRole.Add(userByRole);
                }
            }
            else
            {

                var users = userManager.Users.ToList();
                foreach (var user in users)
                {
                    UserByRole userByRole = new();
                    var roles = await userManager.GetRolesAsync(user);


                    if (user.EmployeeId == 0)
                    {
                        continue;
                    }
                    var emp = await _context.Employees.FindAsync(user.EmployeeId);

                    userByRole.UserId = user.Id;
                    userByRole.Id = emp.Id;
                    userByRole.UserFullName = emp.GetFullName();
                    userByRole.Email = emp.Email;
                    userByRole.EmpCode = emp.EmpCode;
                    userByRole.DOB = emp.DOB;
                    userByRole.DOJ = emp.DOJ;
                    userByRole.Gender = emp.Gender;
                    userByRole.MobileNumber = emp.MobileNumber;
                    userByRole.Department = _context.Departments.Find(emp.DepartmentId).DeptCode + ":" + _context.Departments.Find(emp.DepartmentId).DeptName;
                    userByRole.JobRole = _context.JobRoles.Find(emp.RoleId).RoleCode + ":" + _context.JobRoles.Find(emp.RoleId).RoleName;
                    userByRole.StatusType = _context.StatusTypes.Find(emp.StatusTypeId).Status;
                    foreach (var r in roles)
                    {

                        if (userByRole.AccessRole == null)
                        {
                            userByRole.AccessRole = "";
                        }
                        if (userByRole.AccessRole == "")
                        {
                            userByRole.AccessRole = r;
                        }
                        else
                        {
                            userByRole.AccessRole = userByRole.AccessRole + "," + r;
                        }
                    }
                    ListUserByRole.Add(userByRole);
                }
            }

            return Ok(ListUserByRole);
        }


        [HttpPost]
        [ActionName("GetEmployeesReport")]
        public async Task<IActionResult> GetEmployeesReport(EmployeeSearchModel searchModel)
        {



            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<Employee>();


            if (searchModel.EmployeeId != 0 && searchModel.EmployeeId != null)
                predicate = predicate.And(x => x.Id == searchModel.EmployeeId);
            if (!string.IsNullOrEmpty(searchModel.EmployeeName))
                predicate = predicate.And(x => x.GetFullName().Contains(searchModel.EmployeeName));
            if (!string.IsNullOrEmpty(searchModel.EmpCode))
                predicate = predicate.And(x => x.EmpCode.Contains(searchModel.EmpCode));
            if (!string.IsNullOrEmpty(searchModel.Nationality))
                predicate = predicate.And(x => x.Nationality.Contains(searchModel.Nationality));
            if (!string.IsNullOrEmpty(searchModel.Gender))
                predicate = predicate.And(x => x.Gender.Contains(searchModel.Gender));
            if (searchModel.EmploymentTypeId != 0 && searchModel.EmploymentTypeId != null)
                predicate = predicate.And(x => x.EmploymentTypeId == searchModel.EmploymentTypeId);
            if (searchModel.DepartmentId != 0 && searchModel.DepartmentId != null)
                predicate = predicate.And(x => x.DepartmentId == searchModel.DepartmentId);
            if (searchModel.JobRoleId != 0 && searchModel.JobRoleId != null)
                predicate = predicate.And(x => x.RoleId == searchModel.JobRoleId);
            if (searchModel.ApprovalGroupId != 0 && searchModel.ApprovalGroupId != null)
                predicate = predicate.And(x => x.ApprovalGroupId == searchModel.ApprovalGroupId);
            if (searchModel.StatusTypeId != 0 && searchModel.StatusTypeId != null)
                predicate = predicate.And(x => x.StatusTypeId == searchModel.StatusTypeId);



            List<Employee> result = new();
            if (predicate.IsStarted)
            {
                result = _context.Employees.Where(predicate).ToList();
            }
            else
            {
                result = _context.Employees.ToList();
            }



            List<EmployeeDTO> ListEmployeeDto = new();


            await Task.Run(() =>
            {
                foreach (Employee employee in result)
                {
                    EmployeeDTO employeeDTO = new();

                    employeeDTO.Id = employee.Id;
                    employeeDTO.FullName = employee.GetFullName();
                    employeeDTO.EmpCode = employee.EmpCode;
                    employeeDTO.BankAccount = employee.BankAccount;
                    employeeDTO.BankCardNo = employee.BankCardNo;
                    employeeDTO.PassportNo = employee.PassportNo;
                    employeeDTO.TaxNumber = employee.TaxNumber;
                    employeeDTO.Nationality = employee.Nationality;
                    employeeDTO.DOB = employee.DOB;
                    employeeDTO.DOJ = employee.DOJ;
                    employeeDTO.Gender = employee.Gender;
                    employeeDTO.Email = employee.Email;
                    employeeDTO.MobileNumber = employee.MobileNumber;
                    employeeDTO.EmploymentType = employee.EmploymentTypeId != 0 ? _context.EmploymentTypes.Find(employee.EmploymentTypeId).EmpJobTypeCode + ":" + _context.EmploymentTypes.Find(employee.EmploymentTypeId).EmpJobTypeDesc : string.Empty;
                    employeeDTO.Department = employee.DepartmentId != 0 ? _context.Departments.Find(employee.DepartmentId).DeptCode + ":" + _context.Departments.Find(employee.DepartmentId).DeptName : string.Empty;
                    employeeDTO.JobRole = employee.RoleId != 0 ? _context.JobRoles.Find(employee.RoleId).RoleCode + ":" + _context.JobRoles.Find(employee.RoleId).RoleName : string.Empty;
                    employeeDTO.ApprovalGroup = employeeDTO.ApprovalGroupId != 0 ? _context.ApprovalGroups.Find(employeeDTO.ApprovalGroupId).ApprovalGroupCode : string.Empty;
                    employeeDTO.StatusType = employeeDTO.StatusTypeId != 0 ? _context.StatusTypes.Find(employeeDTO.StatusTypeId).Status : string.Empty;

                    ListEmployeeDto.Add(employeeDTO);
                }
            });

            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[18]
                {
                    //new DataColumn("Id", typeof(int)),
                    new DataColumn("EmployeeId", typeof(int)),
                    new DataColumn("EmployeeFullName", typeof(string)),
                    new DataColumn("EmpCode",typeof(string)),
                    new DataColumn("BankAccount",typeof(string)),
                    new DataColumn("BankCardNo",typeof(string)),
                    new DataColumn("PassportNo",typeof(string)),
                    new DataColumn("TaxNumber",typeof(string)),
                    new DataColumn("Nationality",typeof(string)),
                    new DataColumn("DOB",typeof(DateTime)),
                    new DataColumn("DOJ", typeof(DateTime)),
                    new DataColumn("Gender",typeof(string)),
                    new DataColumn("Email",typeof(string)),
                    new DataColumn("MobileNumber",typeof(string)),
                    new DataColumn("EmploymentType",typeof(string)),
                    new DataColumn("Department",typeof(string)),
                    new DataColumn("JobRole", typeof(string)),
                    new DataColumn("ApprovalGroup", typeof(string)),
                    new DataColumn("StatusType", typeof(string))
                });

            foreach (var emp in ListEmployeeDto)
            {
                dt.Rows.Add(
                    emp.Id,
                    emp.FullName,
                    emp.EmpCode,
                    emp.BankAccount,
                    emp.BankCardNo,
                    emp.PassportNo,
                    emp.TaxNumber,
                    emp.Nationality,
                    emp.DOB,
                    emp.DOJ,
                    emp.Gender,
                    emp.Email,
                    emp.MobileNumber,
                    emp.EmploymentType,
                    emp.Department,
                    emp.JobRole,
                    emp.ApprovalGroup,
                    emp.StatusType
                    );
            }

            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook

            List<string> docUrls = new();
            var docUrl = GetExcel("GetAllEmployees", dt);

            docUrls.Add(docUrl);


            return Ok(docUrls);
        }

        [HttpPost]
        [ActionName("GetEmployeesData")]
        public async Task<IActionResult> GetEmployeesData(EmployeeSearchModel searchModel)
        {

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<Employee>();


            if (searchModel.EmployeeId != 0 && searchModel.EmployeeId != null)
                predicate = predicate.And(x => x.Id == searchModel.EmployeeId);
            if (!string.IsNullOrEmpty(searchModel.EmployeeName))
                predicate = predicate.And(x => x.GetFullName().Contains(searchModel.EmployeeName));
            if (!string.IsNullOrEmpty(searchModel.EmpCode))
                predicate = predicate.And(x => x.EmpCode.Contains(searchModel.EmpCode));
            if (!string.IsNullOrEmpty(searchModel.Nationality))
                predicate = predicate.And(x => x.Nationality.Contains(searchModel.Nationality));
            if (!string.IsNullOrEmpty(searchModel.Gender))
                predicate = predicate.And(x => x.Gender.Contains(searchModel.Gender));
            if (searchModel.EmploymentTypeId != 0 && searchModel.EmploymentTypeId != null)
                predicate = predicate.And(x => x.EmploymentTypeId == searchModel.EmploymentTypeId);
            if (searchModel.DepartmentId != 0 && searchModel.DepartmentId != null)
                predicate = predicate.And(x => x.DepartmentId == searchModel.DepartmentId);
            if (searchModel.JobRoleId != 0 && searchModel.JobRoleId != null)
                predicate = predicate.And(x => x.RoleId == searchModel.JobRoleId);
            if (searchModel.ApprovalGroupId != 0 && searchModel.ApprovalGroupId != null)
                predicate = predicate.And(x => x.ApprovalGroupId == searchModel.ApprovalGroupId);
            if (searchModel.StatusTypeId != 0 && searchModel.StatusTypeId != null)
                predicate = predicate.And(x => x.StatusTypeId == searchModel.StatusTypeId);


            List<Employee> result = new();


            if (predicate.IsStarted)
            {
                result = _context.Employees.Where(predicate).ToList();
            }
            else
            {
                result = _context.Employees.ToList();
            }


            List<EmployeeDTO> ListEmployeeDto = new();
            await Task.Run(() =>
            {

                foreach (Employee employee in result)
                {
                    EmployeeDTO employeeDTO = new();

                    employeeDTO.Id = employee.Id;
                    employeeDTO.FullName = employee.GetFullName();
                    employeeDTO.EmpCode = employee.EmpCode;
                    employeeDTO.BankAccount = employee.BankAccount;
                    employeeDTO.BankCardNo = employee.BankCardNo;
                    employeeDTO.PassportNo = employee.PassportNo;
                    employeeDTO.TaxNumber = employee.TaxNumber;
                    employeeDTO.Nationality = employee.Nationality;
                    employeeDTO.DOB = employee.DOB;
                    employeeDTO.DOJ = employee.DOJ;
                    employeeDTO.Gender = employee.Gender;
                    employeeDTO.Email = employee.Email;
                    employeeDTO.MobileNumber = employee.MobileNumber;
                    employeeDTO.EmploymentType = employee.EmploymentTypeId != 0 ? _context.EmploymentTypes.Find(employee.EmploymentTypeId).EmpJobTypeCode + ":" + _context.EmploymentTypes.Find(employee.EmploymentTypeId).EmpJobTypeDesc : string.Empty;
                    employeeDTO.Department = employee.DepartmentId != 0 ? _context.Departments.Find(employee.DepartmentId).DeptCode + ":" + _context.Departments.Find(employee.DepartmentId).DeptName : string.Empty;
                    employeeDTO.JobRole = employee.RoleId != 0 ? _context.JobRoles.Find(employee.RoleId).RoleCode + ":" + _context.JobRoles.Find(employee.RoleId).RoleName : string.Empty;
                    employeeDTO.ApprovalGroup = employeeDTO.ApprovalGroupId != 0 ? _context.ApprovalGroups.Find(employeeDTO.ApprovalGroupId).ApprovalGroupCode : string.Empty;
                    employeeDTO.StatusType = employeeDTO.StatusTypeId != 0 ? _context.StatusTypes.Find(employeeDTO.StatusTypeId).Status : string.Empty;

                    ListEmployeeDto.Add(employeeDTO);
                }
            });


            return Ok(ListEmployeeDto);
        }


        [HttpPost]
        [ActionName("GetAdvanceAndReimburseReportsEmployeeJson")]

        public async Task<IActionResult> GetAdvanceAndReimburseReportsEmployeeJson(CashAndClaimRequestSearchModel searchModel)
        {
            int? empid = searchModel.EmpId;

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<DisbursementsAndClaimsMaster>();


            if (empid == null)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // particular employee may be manager or any reportee
            if (empid != 0 && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == empid);
            }


            if (searchModel != null)
            {
                //For  string use the below
                //if (!string.IsNullOrEmpty(searchModel.Name))
                //    predicate = predicate.And(x => x.Name.Contains(searchModel.Name));

                if (searchModel.RequestTypeId != 0 && searchModel.RequestTypeId != null)
                    predicate = predicate.And(x => x.RequestTypeId == searchModel.RequestTypeId);
                if (searchModel.DepartmentId != 0 && searchModel.DepartmentId != null)
                    predicate = predicate.And(x => x.DepartmentId == searchModel.DepartmentId);
                if (searchModel.ProjectId != 0 && searchModel.ProjectId != null)
                    predicate = predicate.And(x => x.ProjectId == searchModel.ProjectId);
                if (searchModel.SubProjectId != 0 && searchModel.SubProjectId != null)
                    predicate = predicate.And(x => x.SubProjectId == searchModel.SubProjectId);
                if (searchModel.RecordDateFrom.HasValue)
                    predicate = predicate.And(x => x.RecordDate >= searchModel.RecordDateFrom);
                if (searchModel.RecordDateTo.HasValue)
                    predicate = predicate.And(x => x.RecordDate <= searchModel.RecordDateTo);
                if (searchModel.AmountFrom > 0)
                    predicate = predicate.And(x => x.ClaimAmount >= searchModel.AmountFrom);
                if (searchModel.AmountTo > 0)
                    predicate = predicate.And(x => x.ClaimAmount <= searchModel.AmountTo);
                if (searchModel.IsAccountSettled != null)
                    predicate = predicate.And(x => x.IsSettledAmountCredited == searchModel.IsAccountSettled);
                if (searchModel.CostCenterId != 0 && searchModel.CostCenterId != null)
                    predicate = predicate.And(x => x.CostCenterId == searchModel.CostCenterId);
                if (searchModel.ApprovalStatusId != 0 && searchModel.ApprovalStatusId != null)
                    predicate = predicate.And(x => x.ApprovalStatusId == searchModel.ApprovalStatusId);


                //restrict employees to generate only their content not of other employees
                var result = _context.DisbursementsAndClaimsMasters.Where(predicate).ToList();

                // all employees who report under the manager
                if (empid != 0 && searchModel.IsManager == true)
                {
                    var mgrDeptId = _context.Employees.Find(empid).DepartmentId;
                    List<int> mgrReportees = _context.Employees.Where(e => e.DepartmentId == mgrDeptId && e.Id != empid).Select(s => s.Id).ToList();
                    result = result.Where(r => mgrReportees.Contains(r.EmployeeId)).ToList();
                }


                List<DisbursementsAndClaimsMasterDTO> ListDisbItemsDTO = new();

                await Task.Run(() =>
                {
                    foreach (DisbursementsAndClaimsMaster disb in result)
                    {
                        DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();
                        disbursementsAndClaimsMasterDTO.Id = disb.Id;
                        disbursementsAndClaimsMasterDTO.EmployeeId = disb.EmployeeId;
                        disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disb.EmployeeId).GetFullName();
                        disbursementsAndClaimsMasterDTO.PettyCashRequestId = disb.PettyCashRequestId;
                        disbursementsAndClaimsMasterDTO.ExpenseReimburseReqId = disb.ExpenseReimburseReqId;
                        disbursementsAndClaimsMasterDTO.RequestTypeId = disb.RequestTypeId;
                        disbursementsAndClaimsMasterDTO.RequestType = _context.RequestTypes.Find(disb.RequestTypeId).RequestName;
                        disbursementsAndClaimsMasterDTO.DepartmentId = disb.DepartmentId;
                        disbursementsAndClaimsMasterDTO.Department = disb.DepartmentId != null ? _context.Departments.Find(disb.DepartmentId).DeptCode : null;
                        disbursementsAndClaimsMasterDTO.ProjectId = disb.ProjectId;
                        disbursementsAndClaimsMasterDTO.ProjectName = disb.ProjectId != null ? _context.Projects.Find(disb.ProjectId).ProjectName : null;
                        disbursementsAndClaimsMasterDTO.SubProjectId = disb.SubProjectId;
                        disbursementsAndClaimsMasterDTO.SubProjectName = disb.SubProjectId != null ? _context.SubProjects.Find(disb.SubProjectId).SubProjectName : null;
                        disbursementsAndClaimsMasterDTO.WorkTaskId = disb.WorkTaskId;
                        disbursementsAndClaimsMasterDTO.WorkTaskName = disb.WorkTaskId != null ? _context.WorkTasks.Find(disb.WorkTaskId).TaskName : null;
                        disbursementsAndClaimsMasterDTO.CurrencyTypeId = disb.CurrencyTypeId;
                        disbursementsAndClaimsMasterDTO.CurrencyType = disb.CurrencyTypeId != 0 ? _context.CurrencyTypes.Find(disb.CurrencyTypeId).CurrencyCode : null;
                        disbursementsAndClaimsMasterDTO.ClaimAmount = disb.ClaimAmount;
                        disbursementsAndClaimsMasterDTO.AmountToWallet = disb.AmountToWallet ?? 0;
                        disbursementsAndClaimsMasterDTO.AmountToCredit = disb.AmountToCredit ?? 0;
                        disbursementsAndClaimsMasterDTO.CostCenterId = disb.CostCenterId;
                        disbursementsAndClaimsMasterDTO.CostCenter = disb.CostCenterId != 0 ? _context.CostCenters.Find(disb.CostCenterId).CostCenterCode : null;
                        disbursementsAndClaimsMasterDTO.ApprovalStatusId = disb.ApprovalStatusId;
                        disbursementsAndClaimsMasterDTO.ApprovalStatusType = disb.ApprovalStatusId != 0 ? _context.ApprovalStatusTypes.Find(disb.ApprovalStatusId).Status : null;
                        disbursementsAndClaimsMasterDTO.RecordDate = disb.RecordDate;
                        disbursementsAndClaimsMasterDTO.IsSettledAmountCredited = disb.IsSettledAmountCredited ?? false;
                        disbursementsAndClaimsMasterDTO.SettledDate = disb.SettledDate;
                        disbursementsAndClaimsMasterDTO.SettlementComment = disb.SettlementComment;
                        ListDisbItemsDTO.Add(disbursementsAndClaimsMasterDTO);
                        disbursementsAndClaimsMasterDTO.SettlementAccount = disb.SettlementAccount;
                        disbursementsAndClaimsMasterDTO.SettlementBankCard = disb.SettlementBankCard;
                        disbursementsAndClaimsMasterDTO.AdditionalData = disb.AdditionalData;
                    }

                });
                return Ok(ListDisbItemsDTO);
            }

            return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
        }


        [HttpPost]
        [ActionName("GetAdvanceAndReimburseReportsEmployeeExcel")]

        public async Task<IActionResult> GetAdvanceAndReimburseReportsEmployeeExcel(CashAndClaimRequestSearchModel searchModel)
        {
            int? empid = searchModel.EmpId;

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<DisbursementsAndClaimsMaster>();


            if (empid == null)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // particular employee may be manager or any reportee
            if (empid != 0 && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == empid);
            }

            if (searchModel != null)
            {
                //For  string use the below
                //if (!string.IsNullOrEmpty(searchModel.Name))
                //    predicate = predicate.And(x => x.Name.Contains(searchModel.Name));

                if (searchModel.RequestTypeId != 0 && searchModel.RequestTypeId != null)
                    predicate = predicate.And(x => x.RequestTypeId == searchModel.RequestTypeId);
                if (searchModel.DepartmentId != 0 && searchModel.DepartmentId != null)
                    predicate = predicate.And(x => x.DepartmentId == searchModel.DepartmentId);
                if (searchModel.ProjectId != 0 && searchModel.ProjectId != null)
                    predicate = predicate.And(x => x.ProjectId == searchModel.ProjectId);
                if (searchModel.SubProjectId != 0 && searchModel.SubProjectId != null)
                    predicate = predicate.And(x => x.SubProjectId == searchModel.SubProjectId);
                if (searchModel.RecordDateFrom.HasValue)
                    predicate = predicate.And(x => x.RecordDate >= searchModel.RecordDateFrom);
                if (searchModel.RecordDateTo.HasValue)
                    predicate = predicate.And(x => x.RecordDate <= searchModel.RecordDateTo);
                if (searchModel.AmountFrom > 0)
                    predicate = predicate.And(x => x.ClaimAmount >= searchModel.AmountFrom);
                if (searchModel.AmountTo > 0)
                    predicate = predicate.And(x => x.ClaimAmount <= searchModel.AmountTo);
                if (searchModel.IsAccountSettled != null)
                    predicate = predicate.And(x => x.IsSettledAmountCredited == searchModel.IsAccountSettled);
                if (searchModel.CostCenterId != 0 && searchModel.CostCenterId != null)
                    predicate = predicate.And(x => x.CostCenterId == searchModel.CostCenterId);
                if (searchModel.ApprovalStatusId != 0 && searchModel.ApprovalStatusId != null)
                    predicate = predicate.And(x => x.ApprovalStatusId == searchModel.ApprovalStatusId);



                //restrict employees to generate only their content not of other employees
                var result = _context.DisbursementsAndClaimsMasters.Where(predicate).ToList();

                // all employees who report under the manager
                if (empid != 0 && searchModel.IsManager == true)
                {
                    var mgrDeptId = _context.Employees.Find(empid).DepartmentId;
                    List<int> mgrReportees = _context.Employees.Where(e => e.DepartmentId == mgrDeptId && e.Id != empid).Select(s => s.Id).ToList();
                    result = result.Where(r => mgrReportees.Contains(r.EmployeeId)).ToList();
                }


                List<DisbursementsAndClaimsMasterDTO> ListDisbItemsDTO = new();

                await Task.Run(() =>
                {
                    foreach (DisbursementsAndClaimsMaster disb in result)
                    {
                        DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();
                        disbursementsAndClaimsMasterDTO.Id = disb.Id;
                        disbursementsAndClaimsMasterDTO.EmployeeId = disb.EmployeeId;
                        disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disb.EmployeeId).GetFullName();
                        disbursementsAndClaimsMasterDTO.PettyCashRequestId = disb.PettyCashRequestId;
                        disbursementsAndClaimsMasterDTO.ExpenseReimburseReqId = disb.ExpenseReimburseReqId;
                        disbursementsAndClaimsMasterDTO.RequestTypeId = disb.RequestTypeId;
                        disbursementsAndClaimsMasterDTO.RequestType = _context.RequestTypes.Find(disb.RequestTypeId).RequestName;
                        disbursementsAndClaimsMasterDTO.DepartmentId = disb.DepartmentId;
                        disbursementsAndClaimsMasterDTO.Department = disb.DepartmentId != null ? _context.Departments.Find(disb.DepartmentId).DeptCode : null;
                        disbursementsAndClaimsMasterDTO.ProjectId = disb.ProjectId;
                        disbursementsAndClaimsMasterDTO.ProjectName = disb.ProjectId != null ? _context.Projects.Find(disb.ProjectId).ProjectName : null;
                        disbursementsAndClaimsMasterDTO.SubProjectId = disb.SubProjectId;
                        disbursementsAndClaimsMasterDTO.SubProjectName = disb.SubProjectId != null ? _context.SubProjects.Find(disb.SubProjectId).SubProjectName : null;
                        disbursementsAndClaimsMasterDTO.WorkTaskId = disb.WorkTaskId;
                        disbursementsAndClaimsMasterDTO.WorkTaskName = disb.WorkTaskId != null ? _context.WorkTasks.Find(disb.WorkTaskId).TaskName : null;
                        disbursementsAndClaimsMasterDTO.CurrencyTypeId = disb.CurrencyTypeId;
                        disbursementsAndClaimsMasterDTO.CurrencyType = disb.CurrencyTypeId != 0 ? _context.CurrencyTypes.Find(disb.CurrencyTypeId).CurrencyCode : null;
                        disbursementsAndClaimsMasterDTO.ClaimAmount = disb.ClaimAmount;
                        disbursementsAndClaimsMasterDTO.AmountToWallet = disb.AmountToWallet ?? 0;
                        disbursementsAndClaimsMasterDTO.AmountToCredit = disb.AmountToCredit ?? 0;
                        disbursementsAndClaimsMasterDTO.CostCenterId = disb.CostCenterId;
                        disbursementsAndClaimsMasterDTO.CostCenter = disb.CostCenterId != 0 ? _context.CostCenters.Find(disb.CostCenterId).CostCenterCode : null;
                        disbursementsAndClaimsMasterDTO.ApprovalStatusId = disb.ApprovalStatusId;
                        disbursementsAndClaimsMasterDTO.ApprovalStatusType = disb.ApprovalStatusId != 0 ? _context.ApprovalStatusTypes.Find(disb.ApprovalStatusId).Status : null;
                        disbursementsAndClaimsMasterDTO.RecordDate = disb.RecordDate;
                        disbursementsAndClaimsMasterDTO.IsSettledAmountCredited = disb.IsSettledAmountCredited ?? false;
                        disbursementsAndClaimsMasterDTO.SettledDate = disb.SettledDate;
                        disbursementsAndClaimsMasterDTO.SettlementComment = disb.SettlementComment;
                        disbursementsAndClaimsMasterDTO.SettlementAccount = disb.SettlementAccount;
                        disbursementsAndClaimsMasterDTO.SettlementBankCard = disb.SettlementBankCard;
                        disbursementsAndClaimsMasterDTO.AdditionalData = disb.AdditionalData;
                        ListDisbItemsDTO.Add(disbursementsAndClaimsMasterDTO);
                    }

                });
                //return Ok(ListDisbItemsDTO);


                DataTable dt = new();
                dt.Columns.AddRange(new DataColumn[21]
                    {
                    //new DataColumn("Id", typeof(int)),
                    new DataColumn("EmployeeName", typeof(string)),
                    new DataColumn("PettyCashRequestId", typeof(int)),
                    new DataColumn("ExpenseReimburseReqId", typeof(int)),
                    new DataColumn("RequestType",typeof(string)),
                    new DataColumn("Department",typeof(string)),
                    new DataColumn("Project",typeof(string)),
                    new DataColumn("SubProject", typeof(string)),
                    new DataColumn("WorkTask",typeof(string)),
                    new DataColumn("RecordDate",typeof(DateTime)),
                    new DataColumn("CurrencyType",typeof(string)),
                    new DataColumn("ClaimAmount", typeof(Double)),
                    new DataColumn("AmountToWallet", typeof(Double)),
                    new DataColumn("AmountToCredit", typeof(Double)),
                    new DataColumn("CostCenter", typeof(string)),
                    new DataColumn("ApprovalStatus", typeof(string)),
                    new DataColumn("IsSettledAmountCredited", typeof(bool)),
                    new DataColumn("SettledDate", typeof(DateTime)),
                    new DataColumn("SettlementComment", typeof(string)),
                    new DataColumn("SettlementAccount", typeof(DateTime)),
                    new DataColumn("SettlementBankCard", typeof(DateTime)),
                    new DataColumn("AdditionalData", typeof(DateTime))
                    });


                foreach (var disbItem in ListDisbItemsDTO)
                {
                    dt.Rows.Add(
                        disbItem.EmployeeName,
                        disbItem.PettyCashRequestId,
                        disbItem.ExpenseReimburseReqId,
                        disbItem.RequestType,
                        disbItem.Department,
                        disbItem.ProjectName,
                        disbItem.SubProjectName,
                        disbItem.WorkTaskName,
                        disbItem.RecordDate,
                        disbItem.CurrencyType,
                        disbItem.ClaimAmount,
                        disbItem.AmountToWallet,
                        disbItem.AmountToCredit,
                        disbItem.CostCenter,
                        disbItem.ApprovalStatusType,
                        disbItem.IsSettledAmountCredited,
                        disbItem.SettledDate,
                        disbItem.SettlementComment,
                        disbItem.SettlementAccount,
                        disbItem.SettlementBankCard,
                        disbItem.AdditionalData
                        );
                }
                // Creating the Excel workbook 
                // Add the datatable to the Excel workbook

                List<string> docUrls = new();
                var docUrl = GetExcel("CashReimburseReportByEmployee", dt);

                docUrls.Add(docUrl);

                return Ok(docUrls);
            }

            return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
        }



        [HttpPost]
        [ActionName("GetTravelRequestReportForEmployeeJson")]

        public async Task<IActionResult> GetTravelRequestReportForEmployeeJson(TravelRequestSearchModel searchModel)
        {

            int? empid = searchModel.EmpId;

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<TravelApprovalRequest>();

            if (empid == null)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // particular employee may be manager or any reportee
            if (empid != 0 && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == empid);
            }

            //var emp = await _context.Employees.FindAsync(empid); //employee object
            //string empFullName = emp.GetFullName();

            if (searchModel.TravelApprovalRequestId != 0 && searchModel.TravelApprovalRequestId != null)
                predicate = predicate.And(x => x.Id == searchModel.TravelApprovalRequestId);
            if (searchModel.TravelStartDate.HasValue)
                predicate = predicate.And(x => x.TravelStartDate >= searchModel.TravelStartDate);
            if (searchModel.TravelEndDate.HasValue)
                predicate = predicate.And(x => x.TravelEndDate <= searchModel.TravelEndDate);
            if (!string.IsNullOrEmpty(searchModel.TravelPurpose))
                predicate = predicate.And(x => x.TravelPurpose.Contains(searchModel.TravelPurpose));
            if (searchModel.DepartmentId != 0 && searchModel.DepartmentId != null)
                predicate = predicate.And(x => x.DepartmentId == searchModel.DepartmentId);
            if (searchModel.ProjectId != 0 && searchModel.ProjectId != null)
                predicate = predicate.And(x => x.ProjectId == searchModel.ProjectId);
            if (searchModel.ReqRaisedDate.HasValue)
                predicate = predicate.And(x => x.ReqRaisedDate >= searchModel.ReqRaisedDate);
            if (searchModel.ReqRaisedDate.HasValue)
                predicate = predicate.And(x => x.ReqRaisedDate <= searchModel.ReqRaisedDate);
            if (searchModel.ApprovalStatusTypeId != 0 && searchModel.ApprovalStatusTypeId != null)
                predicate = predicate.And(x => x.ApprovalStatusTypeId == searchModel.ApprovalStatusTypeId);


            //restrict employees to generate only their content not of other employees
            var result = _context.TravelApprovalRequests.Where(predicate).ToList();

            // all employees who report under the manager
            if (empid != 0 && searchModel.IsManager == true)
            {
                int mgrDeptId = _context.Employees.Find(empid).DepartmentId;
                List<int> mgrReportees = _context.Employees.Where(e => e.DepartmentId == mgrDeptId && e.Id != empid).Select(s => s.Id).ToList();
                result = result.Where(r => mgrReportees.Contains(r.EmployeeId)).ToList();
            }


            List<TravelApprovalRequestDTO> ListTravelItemsDTO = new();

            await Task.Run(() =>
            {
                foreach (TravelApprovalRequest travel in result)
                {
                    TravelApprovalRequestDTO travelItemDTO = new();
                    travelItemDTO.Id = travel.Id;
                    travelItemDTO.EmployeeId = travel.EmployeeId;
                    travelItemDTO.EmployeeName = _context.Employees.Find(travel.EmployeeId).GetFullName();

                    travelItemDTO.DepartmentId = travel.DepartmentId;
                    travelItemDTO.Department = travel.DepartmentId != null ? _context.Departments.Find(travel.DepartmentId).DeptName : null;
                    travelItemDTO.ProjectId = travel.ProjectId;
                    travelItemDTO.Project = travel.ProjectId != null ? _context.Projects.Find(travel.ProjectId).ProjectName : null;
                    travelItemDTO.SubProjectId = travel.SubProjectId;
                    travelItemDTO.SubProject = travel.SubProjectId != null ? _context.SubProjects.Find(travel.SubProjectId).SubProjectName : null;
                    travelItemDTO.WorkTaskId = travel.WorkTaskId;
                    travelItemDTO.WorkTask = travel.WorkTaskId != null ? _context.WorkTasks.Find(travel.WorkTaskId).TaskName : null;
                    travelItemDTO.CostCenterId = travel.CostCenterId;
                    travelItemDTO.CostCenter = travel.CostCenterId != 0 ? _context.CostCenters.Find(travel.CostCenterId).CostCenterCode : null;
                    travelItemDTO.ApprovalStatusTypeId = travel.ApprovalStatusTypeId;
                    travelItemDTO.ApprovalStatusType = travel.ApprovalStatusTypeId != 0 ? _context.ApprovalStatusTypes.Find(travel.ApprovalStatusTypeId).Status : null;
                    travelItemDTO.ReqRaisedDate = travel.ReqRaisedDate;

                    ListTravelItemsDTO.Add(travelItemDTO);
                }
            });

            return Ok(ListTravelItemsDTO);

        }



        [HttpPost]
        [ActionName("GetTravelRequestReportForEmployeeExcel")]
        public async Task<IActionResult> GetTravelRequestReportForEmployeeExcel(TravelRequestSearchModel searchModel)
        {

            int? employeeId = searchModel.EmpId;

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<TravelApprovalRequest>();

            if (employeeId == null)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // particular employee may be manager or any reportee
            if (employeeId != 0 && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == searchModel.EmpId);
            }

            //var emp = await _context.Employees.FindAsync(empid); //employee object
            //string empFullName = emp.GetFullName();

            if (searchModel.TravelApprovalRequestId != 0 && searchModel.TravelApprovalRequestId != null)
                predicate = predicate.And(x => x.Id == searchModel.TravelApprovalRequestId);
            if (searchModel.TravelStartDate.HasValue)
                predicate = predicate.And(x => x.TravelStartDate >= searchModel.TravelStartDate);
            if (searchModel.TravelEndDate.HasValue)
                predicate = predicate.And(x => x.TravelEndDate <= searchModel.TravelEndDate);
            if (!string.IsNullOrEmpty(searchModel.TravelPurpose))
                predicate = predicate.And(x => x.TravelPurpose.Contains(searchModel.TravelPurpose));
            if (searchModel.DepartmentId != 0 && searchModel.DepartmentId != null)
                predicate = predicate.And(x => x.DepartmentId == searchModel.DepartmentId);
            if (searchModel.ProjectId != 0 && searchModel.ProjectId != null)
                predicate = predicate.And(x => x.ProjectId == searchModel.ProjectId);
            if (searchModel.ReqRaisedDate.HasValue)
                predicate = predicate.And(x => x.ReqRaisedDate >= searchModel.ReqRaisedDate);
            if (searchModel.ReqRaisedDate.HasValue)
                predicate = predicate.And(x => x.ReqRaisedDate <= searchModel.ReqRaisedDate);
            if (searchModel.ApprovalStatusTypeId != 0 && searchModel.ApprovalStatusTypeId != null)
                predicate = predicate.And(x => x.ApprovalStatusTypeId == searchModel.ApprovalStatusTypeId);


            //restrict employees to generate only their content not of other employees
            var result = _context.TravelApprovalRequests.Where(predicate).ToList();

            // all employees who report under the manager
            if (employeeId != 0 && searchModel.IsManager == true)
            {
                int mgrDeptId = _context.Employees.Find(employeeId).DepartmentId;
                List<int> mgrReportees = _context.Employees.Where(e => e.DepartmentId == mgrDeptId && e.Id != employeeId).Select(s => s.Id).ToList();
                result = result.Where(r => mgrReportees.Contains(r.EmployeeId)).ToList();
            }


            List<TravelApprovalRequestDTO> ListTravelItemsDTO = new();
            await Task.Run(() =>
            {

                foreach (TravelApprovalRequest travel in result)
                {
                    TravelApprovalRequestDTO travelItemDTO = new();
                    travelItemDTO.Id = travel.Id;
                    travelItemDTO.EmployeeId = travel.EmployeeId;
                    travelItemDTO.EmployeeName = _context.Employees.Find(travel.EmployeeId).GetFullName();

                    travelItemDTO.DepartmentId = travel.DepartmentId;
                    travelItemDTO.Department = travel.DepartmentId != null ? _context.Departments.Find(travel.DepartmentId).DeptName : null;
                    travelItemDTO.ProjectId = travel.ProjectId;
                    travelItemDTO.Project = travel.ProjectId != null ? _context.Projects.Find(travel.ProjectId).ProjectName : null;
                    travelItemDTO.SubProjectId = travel.SubProjectId;
                    travelItemDTO.SubProject = travel.SubProjectId != null ? _context.SubProjects.Find(travel.SubProjectId).SubProjectName : null;
                    travelItemDTO.WorkTaskId = travel.WorkTaskId;
                    travelItemDTO.WorkTask = travel.WorkTaskId != null ? _context.WorkTasks.Find(travel.WorkTaskId).TaskName : null;
                    travelItemDTO.CostCenterId = travel.CostCenterId;
                    travelItemDTO.CostCenter = travel.CostCenterId != 0 ? _context.CostCenters.Find(travel.CostCenterId).CostCenterCode : null;
                    travelItemDTO.ApprovalStatusTypeId = travel.ApprovalStatusTypeId;
                    travelItemDTO.ApprovalStatusType = travel.ApprovalStatusTypeId != 0 ? _context.ApprovalStatusTypes.Find(travel.ApprovalStatusTypeId).Status : null;
                    travelItemDTO.ReqRaisedDate = travel.ReqRaisedDate;

                    ListTravelItemsDTO.Add(travelItemDTO);
                }
            });


            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[12]
                {
                    new DataColumn("TravelRequestId", typeof(int)),
                    new DataColumn("EmployeeName", typeof(string)),
                    new DataColumn("TravelStartDate",typeof(string)),
                    new DataColumn("TravelEndDate",typeof(string)),
                    new DataColumn("TravelPurpose",typeof(string)),
                    new DataColumn("Department",typeof(string)),
                    new DataColumn("Project",typeof(string)),
                    new DataColumn("SubProject", typeof(string)),
                    new DataColumn("WorkTask",typeof(string)),
                    new DataColumn("ReqRaisedDate",typeof(DateTime)),
                    new DataColumn("CostCenter", typeof(string)),
                    new DataColumn("ApprovalStatus", typeof(string))
                });

            foreach (var travelItem in ListTravelItemsDTO)
            {
                dt.Rows.Add
                    (
                    travelItem.Id,
                    travelItem.EmployeeName,
                    travelItem.TravelStartDate,
                    travelItem.TravelEndDate,
                    travelItem.TravelPurpose,
                    travelItem.Department,
                    travelItem.Project,
                    travelItem.SubProject,
                    travelItem.WorkTask,
                    travelItem.ReqRaisedDate,
                    travelItem.CostCenter,
                    travelItem.ApprovalStatusType
                    );
            }


            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook

            List<string> docUrls = new();
            var docUrl = GetExcel("TravelRequestReportForEmployee", dt);

            docUrls.Add(docUrl);

            return Ok(docUrls);
        }


        [HttpPost]
        [ActionName("AccountsPayableData")]
        public async Task<ActionResult<IEnumerable<DisbursementsAndClaimsMasterDTO>>> AccountsPayableData(AccountsPayableSearchModel searchModel)
        {

            List<DisbursementsAndClaimsMaster> result = new();
            List<DisbursementsAndClaimsMasterDTO> ListDisbursementsAndClaimsMasterDTO = new();

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<DisbursementsAndClaimsMaster>();


            if (searchModel.IsAccountSettled == true || searchModel.IsAccountSettled == false)
            {
                predicate = predicate.And(x => x.IsSettledAmountCredited == searchModel.IsAccountSettled);
            }

            if (searchModel.SettledAccountsFrom.HasValue)
                predicate = predicate.And(x => x.RecordDate >= searchModel.SettledAccountsFrom);
            if (searchModel.SettledAccountsTo.HasValue)
                predicate = predicate.And(x => x.RecordDate <= searchModel.SettledAccountsTo);

            if (predicate.IsStarted)
            {
                result = _context.DisbursementsAndClaimsMasters.Where(predicate).ToList();
            }
            else
            {
                result = _context.DisbursementsAndClaimsMasters.ToList();
            }


            foreach (DisbursementsAndClaimsMaster disbursementsAndClaimsMaster in result)
            {
                DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();

                disbursementsAndClaimsMasterDTO.Id = disbursementsAndClaimsMaster.Id;
                disbursementsAndClaimsMasterDTO.EmployeeId = disbursementsAndClaimsMaster.EmployeeId;
                disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disbursementsAndClaimsMaster.EmployeeId).GetFullName();
                disbursementsAndClaimsMasterDTO.ExpenseReimburseReqId = disbursementsAndClaimsMaster.ExpenseReimburseReqId;
                disbursementsAndClaimsMasterDTO.DepartmentId = disbursementsAndClaimsMaster.DepartmentId;
                disbursementsAndClaimsMasterDTO.Department = disbursementsAndClaimsMaster.DepartmentId != null ? _context.Departments.Find(disbursementsAndClaimsMaster.DepartmentId).DeptName : string.Empty;
                disbursementsAndClaimsMasterDTO.ProjectId = disbursementsAndClaimsMaster.ProjectId;
                disbursementsAndClaimsMasterDTO.ProjectName = disbursementsAndClaimsMaster.ProjectId != null ? _context.Projects.Find(disbursementsAndClaimsMaster.ProjectId).ProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.SubProjectId = disbursementsAndClaimsMaster.SubProjectId;
                disbursementsAndClaimsMasterDTO.SubProjectName = disbursementsAndClaimsMaster.SubProjectId != null ? _context.SubProjects.Find(disbursementsAndClaimsMaster.SubProjectId).SubProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.WorkTaskId = disbursementsAndClaimsMaster.WorkTaskId;
                disbursementsAndClaimsMasterDTO.WorkTaskName = disbursementsAndClaimsMaster.WorkTaskId != null ? _context.WorkTasks.Find(disbursementsAndClaimsMaster.WorkTaskId).TaskName : string.Empty;
                disbursementsAndClaimsMasterDTO.CurrencyTypeId = disbursementsAndClaimsMaster.CurrencyTypeId;
                disbursementsAndClaimsMasterDTO.CurrencyType = _context.CurrencyTypes.Find(disbursementsAndClaimsMaster.CurrencyTypeId).CurrencyCode;
                disbursementsAndClaimsMasterDTO.RecordDate = disbursementsAndClaimsMaster.RecordDate;
                disbursementsAndClaimsMasterDTO.ClaimAmount = disbursementsAndClaimsMaster.ClaimAmount;
                disbursementsAndClaimsMasterDTO.AmountToWallet = disbursementsAndClaimsMaster.AmountToWallet;
                disbursementsAndClaimsMasterDTO.AmountToCredit = disbursementsAndClaimsMaster.AmountToCredit;
                disbursementsAndClaimsMasterDTO.IsSettledAmountCredited = disbursementsAndClaimsMaster.IsSettledAmountCredited ?? false;
                disbursementsAndClaimsMasterDTO.SettledDate = disbursementsAndClaimsMaster.SettledDate;
                disbursementsAndClaimsMasterDTO.SettlementComment = disbursementsAndClaimsMaster.SettlementComment;
                disbursementsAndClaimsMasterDTO.SettlementAccount = disbursementsAndClaimsMaster.SettlementAccount;
                disbursementsAndClaimsMasterDTO.SettlementBankCard = disbursementsAndClaimsMaster.SettlementBankCard;
                disbursementsAndClaimsMasterDTO.AdditionalData = disbursementsAndClaimsMaster.AdditionalData;
                disbursementsAndClaimsMasterDTO.CostCenterId = disbursementsAndClaimsMaster.CostCenterId;
                disbursementsAndClaimsMasterDTO.ApprovalStatusId = disbursementsAndClaimsMaster.ApprovalStatusId;
                disbursementsAndClaimsMasterDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(disbursementsAndClaimsMaster.ApprovalStatusId).Status;


                ListDisbursementsAndClaimsMasterDTO.Add(disbursementsAndClaimsMasterDTO);

            }

            return Ok(ListDisbursementsAndClaimsMasterDTO);
        }

        // GET: api/DisbursementsAndClaimsMasters
        [HttpPost]
        [ActionName("AccountsPayableReport")]
        public async Task<ActionResult<IEnumerable<DisbursementsAndClaimsMasterDTO>>> AccountsPayableReport(AccountsPayableSearchModel searchModel)
        {
            

            List<DisbursementsAndClaimsMaster> result = new();
            List<DisbursementsAndClaimsMasterDTO> ListDisbursementsAndClaimsMasterDTO = new();

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<DisbursementsAndClaimsMaster>();


            if (searchModel.IsAccountSettled == true || searchModel.IsAccountSettled == false)
            {
                predicate = predicate.And(x => x.IsSettledAmountCredited == searchModel.IsAccountSettled);
            }
            
            if (searchModel.SettledAccountsFrom.HasValue)
                predicate = predicate.And(x => x.RecordDate >= searchModel.SettledAccountsFrom);
            if (searchModel.SettledAccountsTo.HasValue)
                predicate = predicate.And(x => x.RecordDate <= searchModel.SettledAccountsTo);

            if (predicate.IsStarted)
            {
                result =   _context.DisbursementsAndClaimsMasters.Where(predicate).ToList();
            }
            else
            {
                result =  _context.DisbursementsAndClaimsMasters.ToList();
            }


            foreach (DisbursementsAndClaimsMaster disbursementsAndClaimsMaster in result)
            {
                DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();

                disbursementsAndClaimsMasterDTO.Id = disbursementsAndClaimsMaster.Id;
                disbursementsAndClaimsMasterDTO.EmployeeId = disbursementsAndClaimsMaster.EmployeeId;
                disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disbursementsAndClaimsMaster.EmployeeId).GetFullName();
                disbursementsAndClaimsMasterDTO.ExpenseReimburseReqId = disbursementsAndClaimsMaster.ExpenseReimburseReqId;
                disbursementsAndClaimsMasterDTO.DepartmentId = disbursementsAndClaimsMaster.DepartmentId;
                disbursementsAndClaimsMasterDTO.Department = disbursementsAndClaimsMaster.DepartmentId != null ? _context.Departments.Find(disbursementsAndClaimsMaster.DepartmentId).DeptName : string.Empty;
                disbursementsAndClaimsMasterDTO.ProjectId = disbursementsAndClaimsMaster.ProjectId;
                disbursementsAndClaimsMasterDTO.ProjectName = disbursementsAndClaimsMaster.ProjectId != null ? _context.Projects.Find(disbursementsAndClaimsMaster.ProjectId).ProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.SubProjectId = disbursementsAndClaimsMaster.SubProjectId;
                disbursementsAndClaimsMasterDTO.SubProjectName = disbursementsAndClaimsMaster.SubProjectId != null ? _context.SubProjects.Find(disbursementsAndClaimsMaster.SubProjectId).SubProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.WorkTaskId = disbursementsAndClaimsMaster.WorkTaskId;
                disbursementsAndClaimsMasterDTO.WorkTaskName = disbursementsAndClaimsMaster.WorkTaskId != null ? _context.WorkTasks.Find(disbursementsAndClaimsMaster.WorkTaskId).TaskName : string.Empty;
                disbursementsAndClaimsMasterDTO.CurrencyTypeId = disbursementsAndClaimsMaster.CurrencyTypeId;
                disbursementsAndClaimsMasterDTO.CurrencyType = _context.CurrencyTypes.Find(disbursementsAndClaimsMaster.CurrencyTypeId).CurrencyCode;
                disbursementsAndClaimsMasterDTO.RecordDate = disbursementsAndClaimsMaster.RecordDate;
                disbursementsAndClaimsMasterDTO.ClaimAmount = disbursementsAndClaimsMaster.ClaimAmount;
                disbursementsAndClaimsMasterDTO.AmountToWallet = disbursementsAndClaimsMaster.AmountToWallet;
                disbursementsAndClaimsMasterDTO.AmountToCredit = disbursementsAndClaimsMaster.AmountToCredit;
                disbursementsAndClaimsMasterDTO.IsSettledAmountCredited = disbursementsAndClaimsMaster.IsSettledAmountCredited ?? false;
                disbursementsAndClaimsMasterDTO.SettledDate = disbursementsAndClaimsMaster.SettledDate;
                disbursementsAndClaimsMasterDTO.SettlementComment = disbursementsAndClaimsMaster.SettlementComment;
                disbursementsAndClaimsMasterDTO.SettlementAccount = disbursementsAndClaimsMaster.SettlementAccount;
                disbursementsAndClaimsMasterDTO.SettlementBankCard = disbursementsAndClaimsMaster.SettlementBankCard;
                disbursementsAndClaimsMasterDTO.AdditionalData = disbursementsAndClaimsMaster.AdditionalData;
                disbursementsAndClaimsMasterDTO.CostCenterId = disbursementsAndClaimsMaster.CostCenterId;
                disbursementsAndClaimsMasterDTO.ApprovalStatusId = disbursementsAndClaimsMaster.ApprovalStatusId;
                disbursementsAndClaimsMasterDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(disbursementsAndClaimsMaster.ApprovalStatusId).Status;


                ListDisbursementsAndClaimsMasterDTO.Add(disbursementsAndClaimsMasterDTO);

            }


            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[21]
                {
                    //new DataColumn("Id", typeof(int)),
                    new DataColumn("EmployeeName", typeof(string)),
                    new DataColumn("PettyCashRequestId", typeof(int)),
                    new DataColumn("ExpenseReimburseReqId", typeof(int)),
                    new DataColumn("RequestType",typeof(string)),
                    new DataColumn("Department",typeof(string)),
                    new DataColumn("Project",typeof(string)),
                    new DataColumn("SubProject", typeof(string)),
                    new DataColumn("WorkTask",typeof(string)),
                    new DataColumn("RecordDate",typeof(DateTime)),
                    new DataColumn("CurrencyType",typeof(string)),
                    new DataColumn("ClaimAmount", typeof(Double)),
                    new DataColumn("AmountToWallet", typeof(Double)),
                    new DataColumn("AmountToCredit", typeof(Double)),
                    new DataColumn("CostCenter", typeof(string)),
                    new DataColumn("ApprovalStatus", typeof(string)),
                    new DataColumn("IsSettledAmountCredited", typeof(bool)),
                    new DataColumn("SettledDate", typeof(DateTime)),
                    new DataColumn("SettlementComment", typeof(string)),
                    new DataColumn("SettlementAccount", typeof(string)),
                    new DataColumn("SettlementBankCard", typeof(string)),
                    new DataColumn("AdditionalData", typeof(string))
                });


            foreach (var disbItem in ListDisbursementsAndClaimsMasterDTO)
            {
                dt.Rows.Add(
                    disbItem.EmployeeName,
                    disbItem.PettyCashRequestId,
                    disbItem.ExpenseReimburseReqId,
                    disbItem.RequestType,
                    disbItem.Department,
                    disbItem.ProjectName,
                    disbItem.SubProjectName,
                    disbItem.WorkTaskName,
                    disbItem.RecordDate,
                    disbItem.CurrencyType,
                    disbItem.ClaimAmount,
                    disbItem.AmountToWallet,
                    disbItem.AmountToCredit,
                    disbItem.CostCenter,
                    disbItem.ApprovalStatusType,
                    disbItem.IsSettledAmountCredited,
                    disbItem.SettledDate,
                    disbItem.SettlementComment,
                    disbItem.SettlementAccount,
                    disbItem.SettlementBankCard,
                    disbItem.AdditionalData
                    );
            }
            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook

            List<string> docUrls = new();
            var docUrl = GetExcel("AccountsPayableReports", dt);

            docUrls.Add(docUrl);

            return Ok(docUrls);
        }




        private string GetExcel(string reporttype, DataTable dt)
        {
            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook
            using XLWorkbook wb = new();
            wb.Worksheets.Add(dt, reporttype);
            // string xlfileName = reporttype + "_" + DateTime.Now.ToShortDateString().Replace("/", string.Empty) + ".xlsx";
            string xlfileName = reporttype + ".xlsx";

            using MemoryStream stream = new();

            wb.SaveAs(stream);


            string uploadsfolder = Path.Combine(hostingEnvironment.ContentRootPath, "Images");

            string filepath = Path.Combine(uploadsfolder, xlfileName);

            if (System.IO.File.Exists(filepath))
                System.IO.File.Delete(filepath);

            using var outputtream = new FileStream(filepath, FileMode.Create);

            wb.SaveAs(outputtream);

            string docurl = Directory.EnumerateFiles(uploadsfolder).Select(f => filepath).FirstOrDefault().ToString();

            // return File(stream.ToArray(), "Application/Ms-Excel", xlfileName);

            return docurl;

        }
        ///End of methods
    }
}
