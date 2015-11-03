﻿using custom_study_plan_generator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web.Mvc;
using custom_study_plan_generator.MetaObjects;
using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;
using custom_study_plan_generator.App_Start;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using File = Google.Apis.Drive.v2.Data.File;


namespace custom_study_plan_generator.Controllers
{
   
    [Authorize]
	[RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            /* force ASP.NET session initialsiation, needed for Google OAuth */
            Session["dummy"] = "dummy";

            return View();
        }

        public string CheckStudentID()
        {

            using (custom_study_plan_generatorEntities db = new custom_study_plan_generatorEntities())
            {

                
                var data = Request["data"].ToString();
                var dataSplit = data.Split(',');
                var idRaw = dataSplit[0];
                var type = dataSplit[1];
                


                try
                {

                    var id = Convert.ToInt32(idRaw.Substring(1, 7));
                    
                    Session["StudentID"] = idRaw;

                    if (type == "create")
                    {
                        var match = from student in db.Students
                                    where student.student_id == id
                                    select student;

                        if (match.Count() > 0)
                        {
                            return "true";
                        }
                    }

                    else if (type == "edit")
                    {


                        var studentPlan = from sp in db.StudentPlans
                                          where sp.student_id == id
                                          select sp;

                        if (studentPlan.Count() > 0)
                        {
                            int highestPlanID = studentPlan.Max(p => p.plan_id);
                            Session["PlanID"] = highestPlanID;
                            return "true";
                        }
                        else
                        {
                            return "Plan for this student does not exist.";
                        }
                    }
                
                }
                catch (Exception ex) 
                {
                    return "Student ID does not exist.";
                }
            
            

                
            }

            return "Student ID does not exist.";
        }

        public ActionResult DefaultPlan(string courseSelect)
        {

            /* Reset the unit list if the course dropdown list changes */
            if (Session["CurrentCourse"] != null)
            {
                if (courseSelect != Session["CurrentCourse"].ToString())
                {
                    Session["DefaultPlanList"] = null;
                }
            }
            /* Store the currently selected course */
            Session["CurrentCourse"] = courseSelect;

            /* open database so that it will be autmoatically disposed */
            using (custom_study_plan_generatorEntities db = new custom_study_plan_generatorEntities())
            {
                /* Set the default blank course option on page load */
                ViewBag.listValue = "Select Course";

                /* Set the list of course units to be blank (will be tailored to a course when course is selected in view */
                ViewBag.unitListSelected = "";

                /* Initialise the courses list */
                var courseList = new List<string>();

                /* Query the course names from the database */
                var courseQry = from d in db.Courses
                                orderby d.name
                                select d.name;

                /* Add distinct courses to the course list */
                courseList.AddRange(courseQry.Distinct());

                /* Supply the list of courses to the view (to be used in the drop down list) */
                ViewBag.courseSelect = new SelectList(courseList);

                /* Get all available plans */
                var plans = from p in db.DefaultPlans
                            select p;

                /* Get all available units */
                var units = from u in db.Units
                            select u;

                ViewBag.numUnits = 24;

                /* If there has been a course selected and submitted in the drop down list */
                if (!String.IsNullOrEmpty(courseSelect))
                {

                    Session["CourseSelect"] = courseSelect;
                    
                    /* Get the matching course and put it into a meta object */
                    var course = (from c in db.Courses
                                    where c.name == courseSelect
                                    select new CourseDTO
                                    {
                                        course_code = c.course_code,
                                        duration = c.duration,
                                        name = c.name,
                                        num_units = c.num_units,
                                        max_credit = c.max_credit
                                    }).FirstOrDefault();

                    /* Send the number of units to the view for correct table size generation */
                    ViewBag.numUnits = course.num_units;

                    /* Select the plan that matches the meta course */
                    plans = plans.Where(u => u.course_code == course.course_code).OrderBy(u => u.unit_no);

                    /* join the units and plans tables to make them sortable by semester */
                    var query = db.Units.Join(plans, u => u.unit_code, p => p.unit_code, (order, plan) => new { plan.unit_no, order.name });

                    /* sort the query by semester */
                    query = query.OrderBy(u => u.unit_no);

                    /* Convert the matched units to only represent unit names */
                    var unitNamesFiltered = from u in query
                                            select u.name;

                    /* Convert the list of unit names to a seperate list which is usable by eager loading
                        * (This step is needed for when the database is disposed of */
                    var selectedList = new List<string>(unitNamesFiltered);

                    for (var x = 0; x < ViewBag.numUnits; x++)
                    {
                        if (selectedList.ElementAtOrDefault(x) == null)
                        {
                            selectedList.Insert(x, "");
                        }
                    }

                    if (Session["DefaultPlanList"] == null)
                    {
                        /* Pass the unit list to a session variable */
                        Session["DefaultPlanList"] = selectedList;
                    }
                    
                    /* Alert the view that a course has been selected, otherwise a blank page will be loaded */
                    ViewBag.courseSelected = true;

                    /* Check if any of the current units in the unit list are missing their prerequisites */
                    /* ********************************************************************************** */

                    /* Get the list of default plan units from session */
                    var unitList = Session["DefaultPlanList"] as List<string>;
                    
                    /* Initialise two lists required for checking and returning the problem units */
                    List<string> violatedList = new List<string>();
                    List<string> unitsChecked = new List<string>();

                    /* Get the course code from the session stroed selected course */
                    var courseSelected = Session["CourseSelect"].ToString();
                    var courseCode = from c in db.Courses
                                     where c.name == courseSelected
                                     select c.course_code;
                    
                    /* Loop through the unit list */
                    foreach (var unit in unitList)
                    {

                        /* Add current unit to the list of units that have been checked for violations */
                        unitsChecked.Add(unit);

                        /* Get the unit code of the unit currently being checked */
                        var unitToCheck = from u in db.Units
                                          where u.name == unit
                                          select u.unit_code;
                        
                        /* Get the unit prereq codes of the unit being checked (if any) */
                        var prereqs = from p in db.UnitPrerequisites
                                      where unitToCheck.Contains(p.unit_code)
                                      where courseCode.Contains(p.course_code)
                                      select p.prereq_code;

                        /* Convert the prereq codes to unit names */
                        var prereqNames = from u in db.Units
                                          where prereqs.Contains(u.unit_code)
                                          select u.name;
                        
                        /* If the unit has both it's prereqs before it, do nothing, else add it to the violated list */
                        if (prereqNames.Count() > 0)
                        {
                            if (!prereqNames.Except(unitsChecked).Any())
                            {

                            }
                            else
                            {
                                violatedList.Add(unit);
                            }
                        }

                        ViewBag.violatedList = violatedList;

                    }

                }

                else
                {
                    /* No course is selected, load a blank page */
                    ViewBag.courseSelected = false;
                    Session["DefaultPlanList"] = null;
                }

                /* Create a list of all availabe units (at the moment this is aesthetic,
                   this list may actually be hidden from view, but this will prevent an error
                   on selecting no course. This may also be required if a new or incomplete course is loaded 
                   into the view */
                var unitNames = from u in units
                                select u.name;

                /* Convert the unit names to a list, usable by eager loading */
                var list = new List<string>(unitNames);
                /* Sort the list alphabetically */
                list.Sort();
                /* Pass the list to the view */
                ViewBag.unitList = new SelectList(list);

                return View();
            }


        }

        [HttpPost]
        public void DefaultPlanAdd()
        {

            var data = Request["data"].ToString();
            string[] values = data.Split(',');
            var element = Convert.ToInt32(values[0]) - 1;
            var unitName = values[1].ToString();

            var unitList = Session["DefaultPlanList"] as List<string>;

            unitList[element] = unitName;

            Session["DefaultPlanList"] = unitList;
            
        }

        [HttpPost]
        public void DefaultPlanRemove()
        {

            var data = Request["data"].ToString();
            var element = Convert.ToInt32(data) - 1;

            var unitList = Session["DefaultPlanList"] as List<string>;

            unitList[element] = "";

            Session["DefaultPlanList"] = unitList;

        }

        [HttpPost]
        public void DefaultPlanReset()
        {

            Session["DefaultPlanList"] = null;

        }

        [HttpPost]
        public void DefaultPlanSave()
        {

            using (custom_study_plan_generatorEntities db = new custom_study_plan_generatorEntities())
            {

                var unitList = Session["DefaultPlanList"] as List<string>;
                var courseSelect = Session["CourseSelect"].ToString();
                
                Dictionary<Int32, string> listOrdered = new Dictionary<Int32, string>();
                var count = 0;
                foreach (var item in unitList) 
                {
                    listOrdered.Add(count , item);
                    count++;
                }


                
                /* Get the matching course and put it into a meta object */
                var course = from c in db.Courses
                             where c.name == courseSelect
                             select c.course_code;

                var units = from a in listOrdered
                            join u in db.Units
                            on new { name = a.Value } equals
                            new { u.name }
                            orderby a.Key
                            select u;

                var defaultPlan = from dp in db.DefaultPlans
                                  where dp.course_code == course.FirstOrDefault()
                                  select dp;

                foreach (var unit in defaultPlan)
                {
                    db.DefaultPlans.Remove(unit);                 
                }


                count = 1;
                foreach (var u in units)
                {
                    DefaultPlan plan = new DefaultPlan();
                    plan.unit_code = u.unit_code;
                    plan.course_code = course.FirstOrDefault();
                    plan.unit_no = count;
                    plan.semester = (int)Math.Ceiling((double)count/4);
                    count++;
                    db.DefaultPlans.Add(plan);
                }

                db.SaveChanges();

            }
 
        }

        [HttpPost]
        public string GetPrerequisites()
        {

            using (custom_study_plan_generatorEntities db = new custom_study_plan_generatorEntities())
            {
                    
                    /* Get unit name from ajax POST */ 
                    var unit = Request["data"].ToString();

                    var courseCode = Session["CourseCode"].ToString();

                    /* Get the unit code of the unit */
                    var unitToCheck = from u in db.Units
                                      where u.name == unit
                                      select u.unit_code;
                        
                    /* Get the unit prereq codes of the unit being checked (if any) */
                    var prereqs = from p in db.UnitPrerequisites
                                    where unitToCheck.Contains(p.unit_code)
                                    where courseCode.Contains(p.course_code)
                                    select p.prereq_code;

                    /* Convert the prereq codes to unit names */
                    var prereqNames = from u in db.Units
                                        where prereqs.Contains(u.unit_code)
                                        select u.name;

                    var prereqList = "";
                    var count = 0;
                    foreach (var prereq in prereqNames) {
                        if (count == 0) 
                        {
                            prereqList += prereq.ToString();
                        }
                        if (count > 0)
                        {
                            prereqList = prereqList + "," + prereq.ToString();
                        }
                        
                        count++;
                    }

                    return prereqList;
                }
        }

        [HttpPost]
        public ActionResult CreateEdit(string create)
        {

            var formInput = Request["formInput"].ToString();
            if (formInput == "create")
            {
                return RedirectToAction("CreatePlan", "Home");
            }
            else
            {
                Session["FromIndex"] = "true";
                return RedirectToAction("EditPlan", "Home");
            }

        }

        public ActionResult CreatePlan(string courseSelect)
        {

            /* open database so that it will be autmoatically disposed */
            using (custom_study_plan_generatorEntities db = new custom_study_plan_generatorEntities())
            {
                /* Reset any session variables */
                Session["StudentPlan"] = null;
                Session["StudentPlanSwap"] = null;
                Session["numUnits"] = null;
                Session["RemovedExemptions"] = null;
                Session["AlgorithmRun"] = "false";
                Session["StartSemester"] = null;
                Session["CourseCode"] = null;

                /* Set the default blank course option on page load */
                ViewBag.listValue = "Select Course";

                /* Set the list of course units to be blank (will be tailored to a course when course is selected in view */
                ViewBag.unitListSelected = "";

                /* Initialise the courses list */
                var courseList = new List<string>();

                /* Query the course names from the database */
                var courseQry = from d in db.Courses
                                orderby d.name
                                select d.name;

                /* Add distinct courses to the course list */
                courseList.AddRange(courseQry.Distinct());

                /* Supply the list of courses to the view (to be used in the drop down list) */
                ViewBag.courseSelect = new SelectList(courseList);

                /* Get all available plans */
                var plans = from p in db.DefaultPlans
                            select p;

                /* Get all available units */
                var units = from u in db.Units
                            select u;

                ViewBag.numUnits = 24;

                /* If there has been a course selected and submitted in the drop down list */
                if (!String.IsNullOrEmpty(courseSelect))
                {
                    /* Get the matching course and put it into a meta object */
                    var course = (from c in db.Courses
                                  where c.name == courseSelect
                                  select new CourseDTO
                                  {
                                      course_code = c.course_code,
                                      duration = c.duration,
                                      name = c.name,
                                      num_units = c.num_units,
                                      max_credit = c.max_credit
                                  }).FirstOrDefault();

                    /* Send the number of units to the view for correct table size generation */
                    ViewBag.numUnits = course.num_units;
                    Session["numUnits"] = course.num_units;
                    Session["Course"] = course;
                    Session["CourseCode"] = course.course_code;

                    /* Select the plan that matches the meta course */
                    plans = plans.Where(u => u.course_code == course.course_code).OrderBy(u => u.unit_no);

                    /* join the units and plans tables to make them sortable by semester */
                    var query = db.Units.Join(plans, u => u.unit_code, p => p.unit_code, (order, plan) => new { plan.unit_no, order.name });

                    /* sort the query by semester */
                    query = query.OrderBy(u => u.unit_no);

                    /* Convert the matched units to only represent unit names */
                    var unitNamesFiltered = from u in query
                                            select u.name;

                    /* Convert the list of unit names to a seperate list which is usable by eager loading
                     * (This step is needed for when the database is disposed of */
                    var selectedList = new List<string>(unitNamesFiltered);

                    /* Pass the unit list to the view */
                    ViewBag.unitListSelected = selectedList;
                    /* Alert the view that a course has been selected, otherwise a blank page will be loaded */
                    ViewBag.courseSelected = true;

                    /* Create a list of combined unit/default plan (CoursePlan type) objects and add them to a session variable */
                    /* The list will be in order, and accessible by element number */
                    /* The list can be used to track changes to the unit position */

                    var sessionQuery = db.Units.Join(plans, u => u.unit_code, p => p.unit_code, (order, plan) => new CoursePlan { position = plan.unit_no, semester = plan.semester, unit_code = order.unit_code, name = order.name, type_code = order.type_code, semester1 = order.semester1, semester2 = order.semester2, exempt = false, preferred_year = order.preferred_year });

                    sessionQuery = sessionQuery.OrderBy(u => u.position);

                    /* Convert the query to be stored in the session to a list of CoursePlan objects */
                    List<CoursePlan> sessionList = new List<CoursePlan>(sessionQuery);


                    /* Get a list of prerequisites for each unit and add it to the CoursePlan object */
                    for (var x = 0; x < sessionList.Count(); x++)
                    {
                        var unitCode = sessionList[x].unit_code;
                        
                        var prerequisites = from p in db.UnitPrerequisites
                                            where p.course_code == course.course_code
                                            where p.unit_code.Equals(unitCode)
                                            select p.prereq_code;

                        /* ( Convert the prerequisites for this unitCode into as List<string> */
                        List<string> prereqList = prerequisites.ToList();

                        /* Modiy the original list for the session to include the list of prereqs for this unit */
                        sessionList[x].prerequisites = new List<string>(prereqList);       
                    }

                    /* Save the session list to a session variable, ready for use by the view */
                    Session["StudentPlan"] = sessionList;
                }

                else
                {
                    /* No course is selected, load a blank page */
                    ViewBag.courseSelected = false;
                }

                /* Create a list of all availabe units (at the moment this is aesthetic,
                   this list may actually be hidden from view, but this will prevent an error
                   on selecting no course. This may also be required if a new or incomplete course is loaded 
                   into the view */
                var unitNames = from u in units
                                select u.name;

                /* Convert the unit names to a list, usable by eager loading */
                var list = new List<string>(unitNames);
                /* Sort the list alphabetically */
                list.Sort();
                /* Pass the list to the view */
                ViewBag.unitList = new SelectList(list);

                return View();
            }


        }

        [HttpPost]
        public ActionResult Exemptions(string removeExemptions, string next)
        {
            // Check a valid DefaultPlan is in the Session variable.
            if (Session["StudentPlan"] == null)
            {
                // No Course has been selected - Redirect back to the course selection page.
                return RedirectToAction("CreatePlan", "Home");
            }

            /* Retreive the start semester from the form and put it in a session variable, to be used on the Modify page */
            if (Session["StartSemester"] == null)
            {
                var startSemester = Request["startSemester"];
                Session["StartSemester"] = startSemester;
            }

            // If "loadDefault" button is pressed, return the list of updated units to the view.
            if (!string.IsNullOrEmpty(removeExemptions))
            {
                return View();
            }
            /* If "next" button is pressed proceed to "Modify" page */
            else if (!string.IsNullOrEmpty(next))
            {
                return RedirectToAction("Modify", "Home");
            }

            return View();
        }

        [HttpPost]
        public ActionResult RemoveExemptions()
        {
            // Remove exemptions from plan, return true or false.
            // Receives a string of unit id's to remove from the plan in the format of a string: "1,2,3,4,5" etc.
            // Check if Exemptions selected are valid. 
            if (!string.IsNullOrEmpty(Request["data[]"]))
            {
                var data = Request["data[]"].ToString();
                string[] exemptions = data.Split(',');
                int countExistingExempt = 0;
                int totalExempt = 0;

                // Check for previously selected Exemptions removed.
                foreach (CoursePlan unit in (List<CoursePlan>)Session["StudentPlan"])
                {
                    if (unit.exempt == true)
                    {
                        countExistingExempt++;
                    }
                }

                // Add total of exemptions in Session variable and newly selected. 
                totalExempt = countExistingExempt + exemptions.Length;

                // Make sure Exemption Limit has not been reached. 
                if (totalExempt > 0 && totalExempt <= ((CourseDTO)Session["Course"]).max_credit)
                {
                    // Valid number of Exemptions has been selected - mark the Exemptions in the session variable. 
                    foreach (string id in exemptions)
                    {
                        // Convert string to int.
                        int pos = Convert.ToInt32(id);

                        // Mark each selected unit as Exempt in the Plan session variable.
                        foreach (CoursePlan unit in (List<CoursePlan>)Session["StudentPlan"])
                        {
                            if (unit.position == pos)
                            {
                                unit.exempt = true;
                                break;
                            }
                        }
                    }

                    // Return Success.
                    return Content("Exemptions successfully removed.", MediaTypeNames.Text.Plain);
                }
                else
                {
                    // Exemption Limit has been exceeded.
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Content("Exemption Limit exceeded: Please select less units.", MediaTypeNames.Text.Plain);
                }
            }
            else
            {
                // No Exemptions selected.
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content("Please select exemptions to remove.", MediaTypeNames.Text.Plain);
            }
        }

        public ActionResult Modify()
        {
            // Check a valid StudentPlan is in the Session variable.
            if (Session["StudentPlan"] == null)
            {
                // No Course has been selected - Redirect back to the Index page.
                return RedirectToAction("Index", "Home");
            }

            
            // Retrieve sessionList of coursePlan (units) from Session variable
            // StudentPlan.
            List<CoursePlan> sessionList = (List<CoursePlan>)Session["StudentPlan"];
            
            /* Only run the algorithm if it has not already been run. */
            if (Session["AlgorithmRun"].ToString() == "false")
            {

                // Retrieve course length.
                int numUnits = (int)Session["numUnits"];

                // Retrieve if it is midYearStart or not and convert from int to boolean for algorithm.
                bool midYearIntake = false;
                if (Session["StartSemester"] != null && Session["StartSemester"].ToString() == "2")
                {
                    System.Diagnostics.Debug.WriteLine("\n\n\nMID YEAR INTAKE =S TRUE \n\n\n");
                    midYearIntake = true;
                }

                // Create StudyPlanAlgorithm object, that in turn will create course and unit objects.
                //StudyPlanAlgorithm object to pass between methods;
                StudyPlanAlgorithm.StudyPlanAlgorithm algorithm = new StudyPlanAlgorithm.StudyPlanAlgorithm(sessionList, numUnits, midYearIntake);

                // Update sessionList by running algorithm on it.
                sessionList = algorithm.RunAlgorithm(sessionList);
                
               
                // Initialise empty spaces at the end of the plan so that units can be rearranged to any position.
                int courseLength = ((CourseDTO)Session["Course"]).num_units;
                int planLength = sessionList.Count;

                for (int i = planLength; i < courseLength; i++)
                {
                    sessionList.Add(null);
                }

                // Update Session["StudentPlan"]
                Session["StudentPlan"] = sessionList;

                Session["AlgorithmRun"] = "true";
            }

            /* Check if any of the current units in the unit list are missing their prerequisites */
            /* ********************************************************************************** */

            /* Initialise two lists required for checking and returning the problem units */
            List<string> violatedList = new List<string>();
            List<string> unitsChecked = new List<string>();

            using (custom_study_plan_generatorEntities db = new custom_study_plan_generatorEntities())
            {
                
                /* Get the course code from the session stored selected course */
                var courseCode = Session["CourseCode"].ToString();

                List<string> exemptionsList = (List<string>)Session["RemovedExemptions"];
                
                /* If there are any exempt units, add them to list of units that have been checked for violations */
                foreach (var exemption in exemptionsList)
                {
                    unitsChecked.Add(exemption);
                    
                }

                
                /* Loop through the unit list */
                foreach (var unit in sessionList)
                {

                    if (unit != null) {

                        
                        /* Add current unit to the list of units that have been checked for violations */
                        unitsChecked.Add(unit.name);

                        /* Get the unit code of the unit currently being checked */
                        var unitToCheck = from u in db.Units
                                          where u.name == unit.name
                                          select u.unit_code;

                        /* Get the unit prereq codes of the unit being checked (if any) */
                        var prereqs = from p in db.UnitPrerequisites
                                      where unitToCheck.Contains(p.unit_code)
                                      where p.course_code == courseCode
                                      select p.prereq_code;

                        /* Convert the prereq codes to unit names */
                        var prereqNames = from u in db.Units
                                          where prereqs.Contains(u.unit_code)
                                          select u.name;

                        /* If the unit has both it's prereqs before it, do nothing, else add it to the violated list */
                        if (prereqNames.Count() > 0)
                        {
                            if (!prereqNames.Except(unitsChecked).Any())
                            {

                            }
                            else
                            {
                                violatedList.Add(unit.name);
                            }
                        }

                        ViewBag.violatedList = violatedList;
                    }

                }
            }

            return View();
        }


        [HttpPost]
        public void ModifyAdd()
        {

            var data = Request["data"].ToString();
            var dataSplit = data.Split(',');
            var from = dataSplit[0];
            var fromInt = Convert.ToInt32(from) - 1;
            var to = dataSplit[1];
            var toInt = Convert.ToInt32(to) - 1;

            var unitList = Session["StudentPlan"] as List<CoursePlan>;

            if (Session["StudentPlanSwap"] == null)
            {
                List<CoursePlan> studentPlanSwap = new List<CoursePlan>();
                for (var x = 0; x < 12; x++)
                {
                    studentPlanSwap.Add(null);
                }

                Session["StudentPlanSwap"] = studentPlanSwap;
            }
            
            var unitListSwap = Session["StudentPlanSwap"] as List<CoursePlan>;

            unitList[toInt] = unitListSwap[fromInt];
            unitListSwap[fromInt] = null;

            Session["StudentPlan"] = unitList;
            Session["StudentPlanSwap"] = unitListSwap;

        }

        [HttpPost]
        public void ModifyRemove()
        {

            var data = Request["data"].ToString();
            var dataSplit = data.Split(',');
            var from = dataSplit[0];
            var fromInt = Convert.ToInt32(from) - 1;
            var to = dataSplit[1];
            var toInt = Convert.ToInt32(to) - 1;

            var unitList = Session["StudentPlan"] as List<CoursePlan>;
            var courseLength = ((CourseDTO)Session["Course"]).num_units;
            
            if (Session["StudentPlanSwap"] == null)
            {
                List<CoursePlan> studentPlanSwap = new List<CoursePlan>();
                for (var x = 0; x < 12; x++)
                {
                    studentPlanSwap.Add(null);
                }

                Session["StudentPlanSwap"] = studentPlanSwap;
            }

            var unitListSwap = Session["StudentPlanSwap"] as List<CoursePlan>;

            unitListSwap[toInt] = unitList[fromInt];
            unitList[fromInt] = null;
           

            Session["StudentPlan"] = unitList;
            Session["StudentPlanSwap"] = unitListSwap;

        }


        [HttpPost]
        public void ModifyMove()
        {

            var data = Request["data"].ToString();
            var dataSplit = data.Split(',');
            var from = dataSplit[0];
            var fromInt = Convert.ToInt32(from) - 1;
            var to = dataSplit[1];
            var toInt = Convert.ToInt32(to) - 1;

            var unitList = Session["StudentPlan"] as List<CoursePlan>;

            // Move the unit to its new position in the Session plan, mark the old position as null.
            unitList[toInt] = unitList[fromInt];
            unitList[fromInt] = null;

            Session["StudentPlan"] = unitList;

        }


        [HttpPost]
        public void ModifySwap()
        {

            var data = Request["data"].ToString();
            var dataSplit = data.Split(',');
            var from = dataSplit[0];
            var fromInt = Convert.ToInt32(from) - 1;
            var to = dataSplit[1];
            var toInt = Convert.ToInt32(to) - 1;

            var unitListSwap = Session["StudentPlanSwap"] as List<CoursePlan>;

            // Move the unit within the Swap Space, mark the old position as null.
            unitListSwap[toInt] = unitListSwap[fromInt];
            unitListSwap[fromInt] = null;

            Session["StudentPlanSwap"] = unitListSwap;

        }


        public ActionResult EditPlan()
        {

            var fromIndex = Session["FromIndex"].ToString();
     
            /* open database so that it will be autmoatically disposed */
            using (custom_study_plan_generatorEntities db = new custom_study_plan_generatorEntities())
            {
                /* Reset any session variables if the page is coming from the hompage */

                if (fromIndex == "true")
                {

                    Session["numUnits"] = null;
                    Session["RemovedExemptions"] = null;
                    Session["AlgorithmRun"] = "false";
                    Session["StartSemester"] = null;
                    Session["CourseCode"] = null;
                    Session["StudentPlan"] = null;
                    Session["StudentPlanSwap"] = null;
                
                }

                /* Convert the student ID in session to an int */
                var studentIDRaw = Session["StudentID"].ToString();
                var studentID = Convert.ToInt32(studentIDRaw.Substring(1, 7));

                /* Get a list of all the student plans */
                var plans = from p in db.StudentPlans
                            where p.student_id == studentID
                            select p;

                /* Get the id of the most recent plan */
                var newestPlanId = plans.Max(p => p.plan_id);

                /* Select the plan with the newest id */
                var plan = (from p in db.StudentPlans
                            where p.plan_id == newestPlanId
                            select p).FirstOrDefault();

                /* Get a list of all the units in the students plan */
                var units = from u in db.StudentPlanUnits
                            where u.plan_id == plan.plan_id
                            orderby u.unit_no
                            select u;

                /* Find the unit names that match the unit code in the list of units in the students plan */
                var unitNameQuery = db.Units.Join(units, u => u.unit_code, p => p.unit_code, (unitDB, unitP) => new { unitP.unit_no, unitDB.name });

                /* Order the list of unit names by their order number according to the student plan */
                unitNameQuery = unitNameQuery.OrderBy(un => un.unit_no);

                /* Filter query to consist only of unit names */
                var unitNamesFiltered = from un in unitNameQuery
                                        select un.name;

                /* create the unit list */
                var sessionList = new List<string>(unitNamesFiltered);

                /* Get the course name from course identified in StudentPlan */
                var courseName = (from c in db.Courses
                                 where c.course_code == plan.course_code
                                 select c.name).ToString();

                var numUnits = units.Count();
                ViewBag.numUnits = numUnits;
                Session["numUnits"] = numUnits;
                Session["CourseCode"] = plan.course_code;
                Session["CourseName"] = courseName;
                if (fromIndex == "true")
                {
                    Session["StudentPlan"] = sessionList;
                }

                var studentExemptions = from e in db.StudentExemptions
                                        where e.student_id == studentID
                                        select e.unit_code;

                List<string> exemptionsList = (from en in db.Units
                                                     where studentExemptions.Contains(en.unit_code)
                                                     select en.name).ToList();

                Session["RemovedExemptions"] = exemptionsList;

                /* Check if any of the current units in the unit list are missing their prerequisites */
                /* ********************************************************************************** */

                /* Initialise two lists required for checking and returning the problem units */
                List<string> violatedList = new List<string>();
                List<string> unitsChecked = new List<string>();

                

                /* Get the course code from the session stroed selected course */
                var courseCode = Session["CourseCode"].ToString();

                /* If there are any exempt units, add them to list of units that have been checked for violations */
                foreach (var exemption in exemptionsList)
                {
                    unitsChecked.Add(exemption);
                }

                /* Get the updated session list from session */
                sessionList = (List<string>)Session["StudentPlan"];
                
                /* Loop through the unit list */
                foreach (var unit in sessionList)
                {

                    if (unit != null)
                    {

                        /* Add current unit to the list of units that have been checked for violations */
                        unitsChecked.Add(unit);

                        /* Get the unit code of the unit currently being checked */
                        var unitToCheck = from u in db.Units
                                            where u.name == unit
                                            select u.unit_code;

                        /* Get the unit prereq codes of the unit being checked (if any) */
                        var prereqs = from p in db.UnitPrerequisites
                                        where unitToCheck.Contains(p.unit_code)
                                        where p.course_code == courseCode
                                        select p.prereq_code;

                        /* Convert the prereq codes to unit names */
                        var prereqNames = from u in db.Units
                                            where prereqs.Contains(u.unit_code)
                                            select u.name;

                        /* If the unit has both it's prereqs before it, do nothing, else add it to the violated list */
                        if (prereqNames.Count() > 0)
                        {
                            if (!prereqNames.Except(unitsChecked).Any())
                            {
                 
                            }
                            else
                            {
                                violatedList.Add(unit);
                            }
                        }

                        ViewBag.violatedList = violatedList;
                    }

                }

            }

            Session["FromIndex"] = "false";

            return View();
        }

        [HttpPost]
        public void EditAdd()
        {

            var data = Request["data"].ToString();
            var dataSplit = data.Split(',');
            var from = dataSplit[0];
            var fromInt = Convert.ToInt32(from) - 1;
            var to = dataSplit[1];
            var toInt = Convert.ToInt32(to) - 1;

            var unitList = Session["StudentPlan"] as List<string>;
            var unitListSwap = Session["StudentPlanSwap"] as List<string>;

            unitList[toInt] = unitListSwap[fromInt];
            unitListSwap[fromInt] = null;

            Session["StudentPlan"] = unitList;
            Session["StudentPlanSwap"] = unitListSwap;

        }

        [HttpPost]
        public void EditRemove()
        {

            var data = Request["data"].ToString();
            var dataSplit = data.Split(',');
            var from = dataSplit[0];
            var fromInt = Convert.ToInt32(from) - 1;
            var to = dataSplit[1];
            var toInt = Convert.ToInt32(to) - 1;

            var unitList = Session["StudentPlan"] as List<string>;

            if (Session["StudentPlanSwap"] == null)
            {
                List<string> studentPlanSwap = new List<string>();
                for (var x = 0; x < 12; x++)
                {
                    studentPlanSwap.Add(null);
                }

                Session["StudentPlanSwap"] = studentPlanSwap;
            }

            var unitListSwap = Session["StudentPlanSwap"] as List<string>;

            unitListSwap[toInt] = unitList[fromInt];
            unitList[fromInt] = null;

            Session["StudentPlan"] = unitList;
            Session["StudentPlanSwap"] = unitListSwap;

        }

        [HttpPost]
        public void EditReset()
        {
            Session["FromIndex"] = "true";
        }

        public void EditSave()
        {


            /* Retreive required variables from session */
            List<string> sessionList = (List<string>)Session["StudentPlan"];
            List<string> RemovedExemptions = (List<string>)Session["RemovedExemptions"];
            string studentIDRaw = Session["StudentID"].ToString();
            var studentID = Convert.ToInt32(studentIDRaw.Substring(1, 7));
            var courseCode = Session["CourseCode"].ToString();


            using (custom_study_plan_generatorEntities db = new custom_study_plan_generatorEntities())
            {



                /* Remove all existing exemptions before creating new ones */
                var currentExemptions = from ce in db.StudentExemptions
                                        where ce.student_id == studentID
                                        select ce;

                foreach (var exemption in currentExemptions)
                {
                    db.StudentExemptions.Remove(exemption);
                }

                /* Create a list of the new exemptions */
                List<ExemptionModel> studentExemptions = (from unit in db.Units
                                                          where RemovedExemptions.Contains(unit.name)
                                                          select new ExemptionModel() { name = unit.name, unit_code = unit.unit_code }).ToList();
            
                /* Populate the properties of the new exemptions list and add each exemption to the database */
                foreach (var exemption in studentExemptions)
                {
                    StudentExemption se = new StudentExemption();
                    se.student_id = studentID;
                    se.unit_code = exemption.unit_code;
                    se.exempt = true;
                    db.StudentExemptions.Add(se);
                }

                /* Select all the student plans */
                var plans = from plan in db.StudentPlans
                            select plan;

                /* Select the plan with highest id, extract the id, and increment it */
                int highestPlanID = plans.Max(p => p.plan_id);
                var planID = highestPlanID + 1;

                /* Select the plans for current student */
                var studentPlans = from sps in plans
                                   where sps.student_id == studentID
                                   select sps;

                /* select the most recent plan of the plans for the current student */
                var recentPlan = studentPlans.OrderByDescending(o => o.plan_id).FirstOrDefault();

                /* Create a new plan, populate its properties, and add it to the database */
                StudentPlan sp = new StudentPlan();
                sp.plan_id = planID;
                sp.student_id = studentID;
                sp.course_code = courseCode;
                sp.start_semester = recentPlan.start_semester;
                db.StudentPlans.Add(sp);

                /* Get the default plan for this course */
                var defaultPlan = from dp in db.DefaultPlans
                                  where dp.course_code == courseCode
                                  select dp;

                /* join the units and plans tables to make them sortable by semester */
                
                var query = sessionList.Join(db.Units, sl => sl, u => u.name, (unitSL, unitDB) => new { unitDB.unit_code, name = unitSL });
                Debug.WriteLine("QUERY COUNT: " + query.Count());
                var query2 = query.Join(defaultPlan, sl => sl.unit_code, u => u.unit_code, (SL, DP) => new { SL.unit_code, SL.name, DP.semester });
                Debug.WriteLine("QUERY COUNT2: " + query2.Count());
                /* Create a new unit, populate its properties, and add each new unit to the database */
                var count = 1;
                foreach (var unit in query2)
                {
                    StudentPlanUnit spu = new StudentPlanUnit();
                    spu.plan_id = planID;
                    spu.unit_code = unit.unit_code;
                    spu.unit_no = count;
                    spu.semester = unit.semester;
                    count++;
                    db.StudentPlanUnits.Add(spu);
                }

                /* NEED TO ADD A TRY-CATCH */
                db.SaveChanges();

            }

        }

        public ActionResult EditCourse()
        {

            return View();
        }

        public ActionResult Final()
        {

            
        
            return View();
        }

        public void FinalSave()
        {


            /* Retreive required variables from session */
            List<CoursePlan> sessionList = (List<CoursePlan>)Session["StudentPlan"];
            List<string> RemovedExemptions = (List<string>)Session["RemovedExemptions"];
            var startSemester = Convert.ToInt32(Session["StartSemester"]);
            string studentIDRaw = Session["StudentID"].ToString();
            var studentID = Convert.ToInt32(studentIDRaw.Substring(1, 7));
            var courseCode = Session["CourseCode"].ToString();


            using (custom_study_plan_generatorEntities db = new custom_study_plan_generatorEntities())
            {



                /* Remove all existing exemptions before creating new ones */
                var currentExemptions = from ce in db.StudentExemptions
                                        where ce.student_id == studentID
                                        select ce;

                foreach (var exemption in currentExemptions)
                {
                    db.StudentExemptions.Remove(exemption);
                }
                
                /* Create a list of the new exemptions */
                List<ExemptionModel> studentExemptions = (from unit in db.Units
                                        where RemovedExemptions.Contains(unit.name)
                                        select new ExemptionModel() { name = unit.name, unit_code = unit.unit_code}).ToList();
                
                /* Populate the properties of the new exemptions list and add each exemption to the database */
                foreach (var exemption in studentExemptions)
                {
                    StudentExemption se = new StudentExemption();
                    se.student_id = studentID;
                    se.unit_code = exemption.unit_code;
                    se.exempt = true;
                    db.StudentExemptions.Add(se);
                }

                /* Select all the student plans */
                var plans = from plan in db.StudentPlans
                            select plan;

                /* Select the plan with highest id, extract the id, and increment it */
                int highestPlanID = plans.Max(p => p.plan_id);
                var planID = highestPlanID + 1;
                
                /* Create a new plan, populate its properties, and add it to the database */
                StudentPlan sp = new StudentPlan();
                sp.plan_id = planID;
                sp.student_id = studentID;
                sp.course_code = courseCode;
                sp.start_semester = startSemester;
                db.StudentPlans.Add(sp);

                /* Create a new unit, populate its properties, and add each new unit to the database */
                var count = 1;
                foreach (var unit in sessionList)
                {
                    StudentPlanUnit spu = new StudentPlanUnit();
                    spu.plan_id = planID;
                    spu.unit_code = unit.unit_code;
                    spu.unit_no = count;
                    spu.semester = unit.semester;
                    count++;
                    db.StudentPlanUnits.Add(spu);
                }

            /* NEED TO ADD A TRY-CATCH */
            db.SaveChanges();

            uploadPlan(studentExemptions, sessionList, studentID, courseCode, startSemester);
               
            }

            

        }
        
        public ActionResult submitPlan()
        {
       
            return View();
        }
        
        [HttpPost]
        public ActionResult submitPlan(FileModel model)
        {

            if (ModelState.IsValid)
            {
                Session["Step1"] = model;
                return RedirectToAction("submitPlanAsync");
            }

            // errors
            return View(model);
        }

      
        [Authorize]
        public async Task<ActionResult> submitPlanAsync(CancellationToken cancellationToken)
        {
            ViewBag.Message = "Plan Submission Page.";

            var step1 = Session["Step1"] as FileModel;

            Session.Remove("Step1");

            var result = await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).
                    AuthorizeAsync(cancellationToken);

            if (result.Credential == null)
                return new RedirectResult(result.RedirectUri);

            var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = result.Credential,
                ApplicationName = "custom-study-plan-generator"
            });

            var folderListReq = driveService.Files.List();
            folderListReq.Fields = "items/title,items/id";
            // Set query
            folderListReq.Q = "mimeType = 'application/vnd.google-apps.folder' and title ='" + StudyPlanModel.StudyPlanDirectory + "' and trashed = false";
            FileList folderList = await folderListReq.ExecuteAsync();


            File returnedFile = null;

            
            if (folderList.Items.Count >= 1)
            {
                // If multiple folders with StudyPlanModel.StudyPlanDirectory title always choose first one
                File studyPlanFolder = folderList.Items.First();

                // TODO figure out if page token is necessary here
                var fileListReq = driveService.Files.List();
                fileListReq.Fields = "items/title,items/id";
                // Get all spreadsheets in the studyPlanFolder
                fileListReq.Q = "'" + studyPlanFolder.Id + "' in parents and mimeType = 'application/vnd.google-apps.spreadsheet' and trashed = false";
                FileList fileList = await fileListReq.ExecuteAsync();

                returnedFile = StudyPlanModel.generateGoogleSpreadSheet(driveService, step1.Title, studyPlanFolder.Id, fileList);

            }
            else 
            {
                var folder = StudyPlanModel.createDirectory(driveService, StudyPlanModel.StudyPlanDirectory, "RMIT", "root");

                returnedFile = StudyPlanModel.generateGoogleSpreadSheet(driveService, step1.Title, folder.Id);

            }
            // Permission args are currently hardcoded. Uncomment and replace STUDENTNUMBER to enable sharing of the file.
            //StudyPlanModel.addPermission(driveService, returnedFile.Id, STUDENTNUMBER + "@student.rmit.edu.au", "user", "reader");


            //todo...

            // For javascript sharing popup
            ViewBag.UserAccessToken = result.Credential.Token.AccessToken;
            ViewBag.FileId = returnedFile.Id;
            return View(step1);

        }

     
        [Authorize]
        public async Task<ActionResult> driveAsync(CancellationToken cancellationToken)
        {
            ViewBag.Message = "Your drive page.";

            var result = await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).
                    AuthorizeAsync(cancellationToken);

            if (result.Credential == null)
                return new RedirectResult(result.RedirectUri);

            var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = result.Credential,
                ApplicationName = "custom-study-plan-generator"
            });

            var listReq = driveService.Files.List();
            listReq.Fields = "items/title,items/id,items/createdDate,items/downloadUrl,items/exportLinks";
            var list = await listReq.ExecuteAsync();
            var items =
                (from file in list.Items
                 select new FileModel
                 {
                     Title = file.Title,
                     Id = file.Id,
                     CreatedDate = file.CreatedDate,
                     DownloadUrl = file.DownloadUrl ??
                                               (file.ExportLinks != null ? file.ExportLinks["application/pdf"] : null),
                 }).OrderBy(f => f.Title).ToList();
            return View(items);
        }

     
    }

}