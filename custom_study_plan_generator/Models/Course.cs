//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace custom_study_plan_generator.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Course
    {
        public Course()
        {
            this.DefaultPlans = new HashSet<DefaultPlan>();
            this.StudentPlans = new HashSet<StudentPlan>();
        }
    
        public string course_code { get; set; }
        public string name { get; set; }
        public int num_units { get; set; }
        public int duration { get; set; }
    
        public virtual ICollection<DefaultPlan> DefaultPlans { get; set; }
        public virtual ICollection<StudentPlan> StudentPlans { get; set; }
    }
}