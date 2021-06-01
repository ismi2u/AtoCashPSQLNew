using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCash.Models
{
    public class ExpenseSubClaim
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [Required]
        [ForeignKey("ExpenseReimburseRequestId")]
        public virtual ExpenseReimburseRequest ExpenseReimburseRequest { get; set; }
        public int ExpenseReimburseRequestId { get; set; }

        [Required]
        [ForeignKey("ExpenseTypeId")]
        public virtual ExpenseType ExpenseType { get; set; }
        public int ExpenseTypeId { get; set; }

        [Required]
        public Double ExpenseReimbClaimAmount { get; set; }

        public string DocumentIDs { get; set; }


        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string InvoiceNo { get; set; }

        [Required]

        public float Tax { get; set; }

        [Required]
        public double TaxAmount { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }  
        
        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string Vendor { get; set; } 
        
        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public string Location { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(250)")]
        public string Description { get; set; }

    }

    public class ExpenseSubClaimDTO
    {

        public int Id { get; set; }

        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }

        public Double ExpenseReimbClaimAmount { get; set; }

        public string DocumentIDs { get; set; }

        public DateTime ExpReimReqDate { get; set; }

        public string InvoiceNo { get; set; }

        public DateTime InvoiceDate { get; set; }

        public float Tax { get; set; }

        public double TaxAmount { get; set; }

        public string Vendor { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }
        //Foreign Key Relationsions

        public int CurrencyTypeId { get; set; }
        public string CurrencyType { get; set; }
        public int ExpenseTypeId { get; set; }
        public string ExpenseType { get; set; }

        public string Department { get; set; }
        public int? DepartmentId { get; set; }
        public string Project { get; set; }
        public int? ProjectId { get; set; }

        public string SubProject { get; set; }
        public int? SubProjectId { get; set; }

        public string WorkTask { get; set; }
        public int? WorkTaskId { get; set; }


        public string ApprovalStatusType { get; set; }
        public int ApprovalStatusTypeId { get; set; }


        public DateTime? ApprovedDate { get; set; }

    }
}
