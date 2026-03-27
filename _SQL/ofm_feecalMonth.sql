IF OBJECT_ID(N'hulme.ofm_monthfeecalalltot', N'U') IS NOT NULL
	drop table hulme.ofm_monthfeecalalltot;
IF OBJECT_ID(N'hulme.ofm_monthfeecalxreplace', N'U') IS NOT NULL
	drop table hulme.ofm_monthfeecalxreplace;	

select * 
	into hulme.ofm_monthfeecalalltot
		from hulme.ofm_feecalalltot;
		
select *
	into hulme.ofm_monthfeecalxreplace
		from hulme.ofm_feecalxreplace;

truncate table hulme.ofm_feecalalltot;
truncate table hulme.ofm_feecalxreplace;