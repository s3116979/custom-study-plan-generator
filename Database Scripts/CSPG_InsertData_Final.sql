USE [custom_study_plan_generator]
GO

/*** Courses ***/
INSERT INTO [dbo].[Course] ([course_code], [name], [num_units], [max_credit]) VALUES 
('BP094', 'Bachelor of Computer Science', 24, 16),
('BP096', 'Bachelor of Software Engineering', 32, 24),
('BP162', 'Bachelor of Information Technology', 24, 16),
('BP232', 'Bachelor of Technology (Computing Studies)', 24, 16);


/*** Students ***/
INSERT INTO [dbo].[Student] ([student_id], [firstname], [lastname]) VALUES
(1111111, 'Joe', 'Bloggs'),
(1111112, 'Jane', 'Doe'),
(1234567, 'John', 'Smith');


/*** Unit Types ***/
INSERT INTO [dbo].[UnitType] ([type_code], [Description]) VALUES
('c', 'Core Unit'),
('i', 'IT Elective'),
('m', 'Minor Unit'),
('e', 'Student Elective'),
('acs', 'Advanced Computer Science Elective'),
('ait', 'Advanced IT Elective'),
('cs', 'Computer Science Elective'),
('se', 'Software Engineering Elective');


/*** Units ***/
INSERT INTO [dbo].[Unit] ([unit_code], [name], [type_code], [semester1], [semester2]) VALUES
('COSC1073', 'Programming 1', 'c', 1, 1),
('COSC1076', 'Advanced Programming Techniques', 'i', 0, 1),
('COSC1078', 'Introduction to Information Technology', 'c', 1, 1),
('COSC1093', 'Scripting Language Programming', 'i', 1, 0),
('COSC1111', 'Data Communication and Net-Centric Computing', 'i', 0, 1),
('COSC1133', 'Unix Systems Administration and Programming (Linux)', 'i', 0, 1),
('COSC1147', 'Professional Computing Practice', 'c', 0, 1),
('COSC1519', 'Introduction to Programming', 'c', 1, 1),
('COSC2408', 'Programming Project 1', 'c', 1, 1),
('COSC2413', 'Web Programming', 'c', 1, 1),
('COSC2473', 'Introduction to Computer Systems', 'c', 1, 1),
('COSC2536', 'Security in Computing and Information Technology', 'i', 0, 1),
('COSC2625', 'Building IT Systems', 'c', 1, 1),
('COSC2626', 'Cloud Computing', 'i', 0, 1),
('COSC2628', 'User-centred Design', 'c', 1, 1),
('ISYS1057', 'Database Concepts', 'c', 1, 1),
('ISYS1108', 'Software Engineering Project Management', 'c', 1, 1),
('ISYS1118', 'Software Engineering Fundamentals', 'c', 1, 1),
('COSC1284', 'Programming Techniques', 'c', 1, 1),
('COSC2627', 'Discrete Structures in Computing', 'c', 1, 1),
('COSC1107', 'Computing Theory', 'c', 1, 0),
('COSC2299', 'Software Engineering: Process and Tools', 'c', 1, 0),
('COSC2123', 'Algorithms and Analysis', 'c', 1, 1),
('COSC1114', 'Operating Systems', 'c', 0, 1),
('INTE2376-1', 'Approved Industry Experience 1 (Part 1)', 'c', 1, 0),
('INTE2376-2', 'Approved Industry Experience 1 (Part 2)', 'c', 1, 0),
('INTE2376-3', 'Approved Industry Experience 1 (Part 3)', 'c', 1, 0),
('INTE2377-1', 'Approved Industry Experience 2 (Part 1)', 'c', 1, 0),
('INTE2377-2', 'Approved Industry Experience 2 (Part 2)', 'c', 1, 0),
('INTE2377-3', 'Approved Industry Experience 2 (Part 3)', 'c', 1, 0),
('INTE2374', 'Software Engineering: Principles and Practice 1', 'c', 1, 0),
('INTE2375', 'Software Engineering: Principles and Practice 2', 'c', 0, 1),
('COSC2410-1', 'Software Engineering Project A (Part 1)', 'c', 1, 1),
('COSC2410-2', 'Software Engineering Project A (Part 2)', 'c', 1, 1),
('COSC2411-1', 'Software Engineering Project B (Part 1)', 'c', 1, 1),
('COSC2411-2', 'Software Engineering Project B (Part 2)', 'c', 1, 1),
('IT1', 'IT Elective 1', 'i', 1, 1),
('IT2', 'IT Elective 2', 'i', 1, 1),
('IT3', 'IT Elective 3', 'i', 1, 1),
('IT4', 'IT Elective 4', 'i', 1, 1),
('ADVIT1', 'Advanced IT Elective 1', 'ait', 1, 1),
('ADVIT2', 'Advanced IT Elective 2', 'ait', 1, 1),
('ADVIT3', 'Advanced IT Elective 3', 'ait', 1, 1),
('CS1', 'CS Elective 1', 'cs', 1, 1),
('CS2', 'CS Elective 2', 'cs', 1, 1),
('CS3', 'CS Elective 3', 'cs', 1, 1),
('ADVCS1', 'Adv CS Elective 1', 'cs', 1, 1),
('ADVCS2', 'Adv CS Elective 2', 'cs', 1, 1),
('SE1', 'SE Elective 1', 'se', 1, 1),
('SE2', 'SE Elective 2', 'se', 1, 1),
('MINOR1', 'Minor Course 1', 'm', 1, 1),
('MINOR2', 'Minor Course 2', 'm', 1, 1),
('MINOR3', 'Minor Course 3', 'm', 1, 1),
('MINOR4', 'Minor Course 4', 'm', 1, 1),
('ELECTIVE1', 'Student Elective 1', 'e', 1, 1),
('ELECTIVE2', 'Student Elective 2', 'e', 1, 1),
('ELECTIVE3', 'Student Elective 3', 'e', 1, 1),
('ELECTIVE4', 'Student Elective 4', 'e', 1, 1),
('ELECTIVE5', 'Student Elective 5', 'e', 1, 1);


/*** Unit Prerequisites ***/
INSERT INTO [dbo].[UnitPrerequisites] ([course_code], [unit_code], [prereq_code], [mutiple_required]) VALUES
('BP232', 'COSC1073', 'COSC1519', 0),
('BP232', 'COSC2413', 'COSC1519', 0),
('BP232', 'ISYS1118', 'COSC1073', 0),
('BP232', 'COSC1147', 'ISYS1118', 0),
('BP232', 'ISYS1108', 'ISYS1118', 0),
('BP232', 'COSC2408', 'ISYS1108', 0),
('BP162', 'COSC1073', 'COSC1519', 0),
('BP162', 'COSC2413', 'COSC1519', 0),
('BP162', 'ISYS1118', 'COSC1073', 0),
('BP162', 'COSC2536', 'COSC1111', 0),
('BP162', 'COSC1147', 'ISYS1118', 0),
('BP162', 'ISYS1108', 'ISYS1118', 0),
('BP162', 'COSC2408', 'ISYS1108', 0),
('BP094', 'COSC2413', 'COSC1284', 0),
('BP094', 'COSC1076', 'COSC1284', 0),
('BP094', 'ISYS1118', 'COSC1284', 0),
('BP094', 'COSC1107', 'COSC2627', 1),
('BP094', 'COSC1107', 'COSC1284', 1),
('BP094', 'COSC2299', 'ISYS1118', 1),
('BP094', 'COSC2299', 'COSC1284', 1),
('BP094', 'COSC2536', 'COSC1111', 0),
('BP094', 'COSC1147', 'ISYS1118', 0),
('BP094', 'COSC2123', 'COSC1076', 0),
('BP094', 'COSC1114', 'COSC1076', 0),
('BP094', 'ISYS1108', 'ISYS1118', 0),
('BP094', 'COSC2408', 'ISYS1108', 0),
('BP096', 'COSC2413', 'COSC1284', 0),
('BP096', 'COSC1076', 'COSC1284', 0),
('BP096', 'ISYS1118', 'COSC1284', 0),
('BP096', 'COSC1107', 'COSC2627', 1),
('BP096', 'COSC1107', 'COSC1284', 1),
('BP096', 'COSC2299', 'ISYS1118', 1),
('BP096', 'COSC2299', 'COSC1284', 1),
('BP096', 'COSC2536', 'COSC1111', 0),
('BP096', 'COSC1147', 'ISYS1118', 0),
('BP096', 'COSC2123', 'COSC1076', 0),
('BP096', 'COSC1114', 'COSC1076', 0),
('BP096', 'COSC2410-1', 'INTE2375', 1),
('BP096', 'COSC2410-1', 'INTE2377-3', 1),
('BP096', 'COSC2410-2', 'COSC2410-1', 1),
('BP096', 'COSC2411-1', 'INTE2375', 1),
('BP096', 'COSC2411-1', 'INTE2377-3', 1),
('BP096', 'COSC2411-2', 'COSC2411-1', 1),
('BP096', 'INTE2376-1', 'COSC2628', 1),
('BP096', 'INTE2376-1', 'COSC1284', 1),
('BP096', 'INTE2376-1', 'COSC2473', 1),
('BP096', 'INTE2376-1', 'COSC2627', 1),
('BP096', 'INTE2376-1', 'COSC1076', 1),
('BP096', 'INTE2376-1', 'ISYS1118', 1),
('BP096', 'INTE2376-1', 'COSC1111', 1),
('BP096', 'INTE2376-1', 'COSC2413', 1),
('BP096', 'INTE2376-1', 'ISYS1057', 1),
('BP096', 'INTE2376-1', 'COSC1107', 1),
('BP096', 'INTE2376-1', 'COSC2299', 1),
('BP096', 'INTE2376-1', 'CS1', 1),
('BP096', 'INTE2376-1', 'COSC2123', 1),
('BP096', 'INTE2376-1', 'COSC1114', 1),
('BP096', 'INTE2376-1', 'COSC1147', 1),
('BP096', 'INTE2376-1', 'COSC2536', 1),
('BP096', 'INTE2376-2', 'INTE2376-1', 1),
('BP096', 'INTE2376-3', 'INTE2376-2', 1),
('BP096', 'INTE2377-1', 'COSC2628', 1),
('BP096', 'INTE2377-1', 'COSC1284', 1),
('BP096', 'INTE2377-1', 'COSC2473', 1),
('BP096', 'INTE2377-1', 'COSC2627', 1),
('BP096', 'INTE2377-1', 'COSC1076', 1),
('BP096', 'INTE2377-1', 'ISYS1118', 1),
('BP096', 'INTE2377-1', 'COSC1111', 1),
('BP096', 'INTE2377-1', 'COSC2413', 1),
('BP096', 'INTE2377-1', 'ISYS1057', 1),
('BP096', 'INTE2377-1', 'COSC1107', 1),
('BP096', 'INTE2377-1', 'COSC2299', 1),
('BP096', 'INTE2377-1', 'CS1', 1),
('BP096', 'INTE2377-1', 'COSC2123', 1),
('BP096', 'INTE2377-1', 'COSC1114', 1),
('BP096', 'INTE2377-1', 'COSC1147', 1),
('BP096', 'INTE2377-1', 'COSC2536', 1),
('BP096', 'INTE2377-2', 'INTE2377-1', 1),
('BP096', 'INTE2377-3', 'INTE2377-2', 1),
('BP096', 'INTE2374', 'COSC2628', 1),
('BP096', 'INTE2374', 'COSC1284', 1),
('BP096', 'INTE2374', 'COSC2473', 1),
('BP096', 'INTE2374', 'COSC2627', 1),
('BP096', 'INTE2374', 'COSC1076', 1),
('BP096', 'INTE2374', 'ISYS1118', 1),
('BP096', 'INTE2374', 'COSC1111', 1),
('BP096', 'INTE2374', 'COSC2413', 1),
('BP096', 'INTE2374', 'ISYS1057', 1),
('BP096', 'INTE2374', 'COSC1107', 1),
('BP096', 'INTE2374', 'COSC2299', 1),
('BP096', 'INTE2374', 'CS1', 1),
('BP096', 'INTE2374', 'COSC2123', 1),
('BP096', 'INTE2374', 'COSC1114', 1),
('BP096', 'INTE2374', 'COSC1147', 1),
('BP096', 'INTE2374', 'COSC2536', 1),
('BP096', 'INTE2375', 'COSC2628', 1),
('BP096', 'INTE2375', 'COSC1284', 1),
('BP096', 'INTE2375', 'COSC2473', 1),
('BP096', 'INTE2375', 'COSC2627', 1),
('BP096', 'INTE2375', 'COSC1076', 1),
('BP096', 'INTE2375', 'ISYS1118', 1),
('BP096', 'INTE2375', 'COSC1111', 1),
('BP096', 'INTE2375', 'COSC2413', 1),
('BP096', 'INTE2375', 'ISYS1057', 1),
('BP096', 'INTE2375', 'COSC1107', 1),
('BP096', 'INTE2375', 'COSC2299', 1),
('BP096', 'INTE2375', 'CS1', 1),
('BP096', 'INTE2375', 'COSC2123', 1),
('BP096', 'INTE2375', 'COSC1114', 1),
('BP096', 'INTE2375', 'COSC1147', 1),
('BP096', 'INTE2375', 'COSC2536', 1);



/*** Default Plan ***/
INSERT INTO [dbo].[DefaultPlan] ([course_code], [unit_no], [unit_code], [semester]) VALUES
('BP232', 1, 'COSC1519', 1),
('BP232', 2, 'COSC1078', 1),
('BP232', 3, 'COSC2628', 1),
('BP232', 4, 'COSC2625', 1),
('BP232', 5, 'COSC1073', 2),
('BP232', 6, 'ELECTIVE1', 2),
('BP232', 7, 'ELECTIVE2', 2),
('BP232', 8, 'ELECTIVE3', 2),
('BP232', 9, 'ISYS1057', 3),
('BP232', 10, 'IT1', 3),
('BP232', 11, 'COSC2413', 3),
('BP232', 12, 'MINOR1', 3),
('BP232', 13, 'ISYS1118', 4),
('BP232', 14, 'ADVIT1', 4),
('BP232', 15, 'ELECTIVE4', 4),
('BP232', 16, 'MINOR2', 4),
('BP232', 17, 'COSC1147', 5),
('BP232', 18, 'ISYS1108', 5),
('BP232', 19, 'ADVIT2', 5),
('BP232', 20, 'MINOR3', 5),
('BP232', 21, 'COSC2408', 6),
('BP232', 22, 'ADVIT3', 6),
('BP232', 23, 'ELECTIVE5', 6),
('BP232', 24, 'MINOR4', 6),
('BP162', 1, 'COSC1519', 1),
('BP162', 2, 'COSC1078', 1),
('BP162', 3, 'COSC2628', 1),
('BP162', 4, 'COSC2625', 1),
('BP162', 5, 'COSC1073', 2),
('BP162', 6, 'COSC2413', 2),
('BP162', 7, 'COSC2473', 2),
('BP162', 8, 'COSC1111', 2),
('BP162', 9, 'ISYS1057', 3),
('BP162', 10, 'ISYS1118', 3),
('BP162', 11, 'IT1', 3),
('BP162', 12, 'MINOR1', 3),
('BP162', 13, 'COSC2536', 4),
('BP162', 14, 'COSC1147', 4),
('BP162', 15, 'IT2', 4),
('BP162', 16, 'MINOR2', 4),
('BP162', 17, 'ISYS1108', 5),
('BP162', 18, 'ELECTIVE1', 5),
('BP162', 19, 'IT3', 5),
('BP162', 20, 'MINOR3', 5),
('BP162', 21, 'COSC2408', 6),
('BP162', 22, 'ELECTIVE2', 6),
('BP162', 23, 'IT4', 6),
('BP162', 24, 'MINOR4', 6),
('BP094', 1, 'COSC2628', 1),
('BP094', 2, 'COSC1284', 1),
('BP094', 3, 'COSC2473', 1),
('BP094', 4, 'COSC2627', 1),
('BP094', 5, 'COSC1076', 2),
('BP094', 6, 'ISYS1118', 2),
('BP094', 7, 'COSC1111', 2),
('BP094', 8, 'COSC2413', 2),
('BP094', 9, 'ISYS1057', 3),
('BP094', 10, 'COSC1107', 3),
('BP094', 11, 'COSC2299', 3),
('BP094', 12, 'CS1', 3),
('BP094', 13, 'COSC2123', 4),
('BP094', 14, 'COSC1114', 4),
('BP094', 15, 'COSC1147', 4),
('BP094', 16, 'COSC2536', 4),
('BP094', 17, 'ISYS1108', 5),
('BP094', 18, 'ADVCS1', 5),
('BP094', 19, 'CS2', 5),
('BP094', 20, 'ELECTIVE1', 5),
('BP094', 21, 'COSC2408', 6),
('BP094', 22, 'ADVCS2', 6),
('BP094', 23, 'CS3', 6),
('BP094', 24, 'ELECTIVE2', 6),
('BP096', 1, 'COSC2628', 1),
('BP096', 2, 'COSC1284', 1),
('BP096', 3, 'COSC2473', 1),
('BP096', 4, 'COSC2627', 1),
('BP096', 5, 'COSC1076', 2),
('BP096', 6, 'ISYS1118', 2),
('BP096', 7, 'COSC1111', 2),
('BP096', 8, 'COSC2413', 2),
('BP096', 9, 'ISYS1057', 3),
('BP096', 10, 'COSC1107', 3),
('BP096', 11, 'COSC2299', 3),
('BP096', 12, 'CS1', 3),
('BP096', 13, 'COSC2123', 4),
('BP096', 14, 'COSC1114', 4),
('BP096', 15, 'COSC1147', 4),
('BP096', 16, 'COSC2536', 4),
('BP096', 17, 'INTE2376-1', 5),
('BP096', 18, 'INTE2376-2', 5),
('BP096', 19, 'INTE2376-3', 5),
('BP096', 20, 'INTE2374', 5),
('BP096', 21, 'INTE2377-1', 6),
('BP096', 22, 'INTE2377-2', 6),
('BP096', 23, 'INTE2377-3', 6),
('BP096', 24, 'INTE2375', 6),
('BP096', 25, 'COSC2410-1', 5),
('BP096', 26, 'COSC2410-2', 5),
('BP096', 27, 'SE1', 5),
('BP096', 28, 'ELECTIVE1', 5),
('BP096', 29, 'COSC2411-1', 6),
('BP096', 30, 'COSC2411-2', 6),
('BP096', 31, 'SE2', 6),
('BP096', 32, 'ELECTIVE2', 6);


/*** Student Exemptions ***/
INSERT INTO [dbo].[StudentExemptions] ([student_id], [unit_code], [exempt]) VALUES
(1234567, 'COSC1078', 1),
(1234567, 'COSC1519', 1),
(1234567, 'COSC2625', 1),
(1234567, 'ELECTIVE1', 1);


/*** Student Plan ***/
INSERT INTO [dbo].[StudentPlan] ([plan_id], [student_id], [course_code], [start_semester]) VALUES
(1, 1234567, 'BP232', 1),
(2, 1111111, 'BP232', 1),
(3, 1111112, 'BP232', 2);


/*** Student Plan Units ***/
INSERT INTO [dbo].[StudentPlanUnits] ([plan_id], [unit_no], [unit_code], [semester]) VALUES
(1, 1, 'COSC1073', 1),
(1, 2, 'COSC2413', 1),
(1, 3, 'COSC2628', 1),
(1, 4, 'COSC1111', 1),
(1, 5, 'ISYS1057', 2),
(1, 6, 'ISYS1118', 2),
(1, 7, 'COSC2473', 2),
(1, 8, 'MINOR1', 2),
(1, 9, 'ISYS1108', 3),
(1, 10, 'IT2', 3),
(1, 11, 'IT1', 3),
(1, 12, 'MINOR2', 3),
(1, 13, 'COSC2536', 4),
(1, 14, 'COSC1147', 4),
(1, 15, 'IT3', 4),
(1, 16, 'MINOR3', 4),
(1, 17, 'COSC2408', 5),
(1, 18, 'ELECTIVE1', 5),
(1, 19, 'IT4', 5),
(1, 20, 'MINOR4', 5);
 

GO