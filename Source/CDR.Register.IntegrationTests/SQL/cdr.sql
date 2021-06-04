
-- Get legalentity/participation/brand/softwareproduct status
select 
   p.legalentityid, le.legalentityname,
   p.participationid, ps.participationstatuscode, pt.participationtypecode, it.industrytypecode,
   b.brandid, b.brandname, bs.brandstatuscode,
   sp.softwareproductid, sp.softwareproductname, sps.softwareproductstatuscode
from 
   brand b
   left outer join brandstatus bs on bs.brandstatusid = b.brandstatusid
   left outer join softwareproduct sp on sp.brandid = b.brandid
   left outer join softwareproductstatus sps on sps.softwareproductstatusid = sp.statusid
   left outer join participation p on p.participationid = b.participationid
   left outer join participationstatus ps on ps.participationstatusid = p.statusid
   left outer join participationtype pt on pt.participationtypeid = p.participationtypeid
   left outer join industrytype it on it.industrytypeid = p.industryid
   left outer join legalentity le on le.legalentityid = p.legalentityid
where 
--   sp.softwareproductid not null   
  pt.participationtypecode = "DR"
order by 
   le.legalentityid, b.brandid, sp.softwareproductid;      


-- legalentity/participation status
select 
   p.legalentityid, le.legalentityname,
   p.participationid, ps.participationstatuscode, pt.participationtypecode,
   it.industrytypecode
from
   participation p
   left outer join participationstatus ps on ps.participationstatusid = p.statusid
   left outer join participationtype pt on pt.participationtypeid = p.participationtypeid
   left outer join industrytype it on it.industrytypeid = p.industryid
   left outer join legalentity le on le.legalentityid = p.legalentityid
order by
   le.legalentityid, p.participationid;   


-- softwareproduct status
select 
   sp.softwareproductid, sp.softwareproductname, sps.softwareproductstatuscode
from 
   softwareproduct sp 
   left outer join softwareproductstatus sps on sps.softwareproductstatusid = sp.statusid
order by
   sp.softwareproductid;   


-- brand status
select
   b.brandid, b.brandname, 
   bs.brandstatuscode 
from
   brand b
   left outer join brandstatus bs on bs.brandstatusid = b.brandstatusid
order by
   b.brandid;   
