# County Classification

Classifies the USA Counties in all states into Rural Urban using the RUCC 2013 report.

Columns:

```
StateCode  : 2 character US State codes
FIPS       : Full FIPS Code <STATE_FIPS || COUNTY_FIPS>
County     : Name of the county
RU_Detail  : Rural Urban RUCC2013 Data
M_or_NM    : Metropolitan or Non-metropolitan area classification
```

`RU_Detail` numeric scheme definition:

```
Metropolitan Counties*			
Code	Description		
1	Counties in metro areas of 1 million population or more		
2	Counties in metro areas of 250,000 to 1 million population		
3	Counties in metro areas of fewer than 250,000 population		
			
Nonmetropolitan Counties			
Code	Description		
4	Urban population of 20,000 or more, adjacent to a metro area		
5	Urban population of 20,000 or more, not adjacent to a metro area		
6	Urban population of 2,500 to 19,999, adjacent to a metro area		
7	Urban population of 2,500 to 19,999, not adjacent to a metro area		
8	Completely rural or less than 2,500 urban population, adjacent to a metro area		
9	Completely rural or less than 2,500 urban population, not adjacent to a metro area		
```
