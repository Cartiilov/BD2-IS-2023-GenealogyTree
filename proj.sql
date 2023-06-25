
Create database bd2project;

use bd2project;
go


 DROP TABLE PERSON;
 

 create Table Person
 (id INT PRIMARY KEY,
 mid INT,
 family_id INT,
 firstname VARCHAR(50),
 lastname VARCHAR(50),
 dateofbirth DATE,
 gender VARCHAR(7),
 gen HIERARCHYID,
 FOREIGN KEY(mid) references Person(id)
 );

 drop procedure GetProperId;

CREATE function GetProperId()
returns int
AS
BEGIN
	RETURN
	(
		CASE
			WHEN NOT EXISTS(SELECT * FROM person WHERE id = 1) THEN 1
			ELSE
			(
				SELECT MIN(A.id + 1)
				FROM PERSON AS A
				WHERE NOT EXISTS
				(
					SELECT * FROM person AS B
					WHERE B.id = A.id + 1
				)
			)
		END
	);
END


 delete from person;

 DROP PROCEDURE AddPerson;

CREATE PROCEDURE AddPerson
	@fname VARCHAR(50), @lname VARCHAR(50), @dob varchar(20), @gender VARCHAR(7)
as
begin
	SET IDENTITY_INSERT PERSON ON;
	DECLARE @id AS INT;
	select @id = dbo.GetProperId();
	INSERT INTO PERSON (id, firstname, lastname, dateofbirth, gender) values (@id, @fname, @lname, cast(@dob as DATE) , @gender);
	SET IDENTITY_INSERT PERSON OFF;
end

EXEC AddPerson @fname = 'prawnuczka z rodu corki', @lname ='inna', @dob= '1989-07-27', @gender='f';


select * from person;

DROP PROCEDURE GetAllPeople;
CREATE PROCEDURE GetAllPeople
AS
BEGIN
	SELECT id, firstname, lastname from person
END

EXEC GetAllPeople;

DROP PROCEDURE GetPerson;
CREATE PROCEDURE GetPerson
	@id INT
AS
BEGIN
	SELECT id, firstname, lastname, dateofbirth, gender from person where id = @id
END

DROP PROCEDURE UpdateHidDescendants;
DROP PROCEDURE AddParent;


CREATE PROCEDURE UpdateHidDescendants
	@id INT, @newhid varchar(50), @newfid INT
as
begin
	DECLARE @gen HIERARCHYID, @fid INT
	SET @gen = (SELECT GEN FROM PERSON WHERE ID = @id);
	SET @fid = (SELECT family_id FROM PERSON WHERE ID = @id);
	update person set gen = CONCAT(@newhid, gen.ToString()), family_id = @newfid where gen.IsDescendantOf(@gen) = 1 and family_id = @fid;
end


CREATE PROCEDURE AddParent
	@idp INT, @idc INT
as
BEGIN
	DECLARE @pgender VARCHAR(7), @cgender VARCHAR(7), @newgen VARCHAR(50)
	SET @pgender = (SELECT GENDER FROM PERSON WHERE ID = @idp);
	SET @cgender = (SELECT GENDER FROM PERSON WHERE ID = @idc);

	IF @pgender LIKE 'f'
		UPDATE PERSON SET MID = @idp WHERE ID = @idc;
	ELSE
	BEGIN
		DECLARE @phid HIERARCHYID, @rootid int;
		SET @phid = (SELECT gen FROM PERSON WHERE ID = @idp);
		IF @phid IS NOT NULL
		BEGIN
			DECLARE @numofchildren INT;
			SET @rootid = (SELECT family_id from person where id = @idp);
			SET @numofchildren = (select COUNT(*) from person where gen.IsDescendantOf(@phid)=1 and family_id=@rootid and id != @idp and (gen.GetLevel() - @phid.GetLevel() < 2));
			SET @newgen = CONCAT(@phid.ToString(), @numofchildren + 1);
		END
		ELSE BEGIN
			SET @rootid = @idp;
			UPDATE PERSON SET family_id = @idp, gen = '/' where id = @idp;
			SET @newgen = '/1';
		END
		EXEC UpdateHidDescendants @id = @idc, @newhid = @newgen, @newfid = @rootid;
		SET @newgen = CONCAT(@newgen, '/');
		UPDATE PERSON SET gen = @newgen, family_id = @rootid where id = @idc;
	END
END

DROP PROCEDURE AddParent;
DROP PROCEDURE RemovePerson;

CREATE PROCEDURE RemovePerson
	@id INT
AS
BEGIN
	declare @gender VARCHAR(7)
	SET @gender = (SELECT GENDER FROM PERSON WHERE id = @id);
	IF @gender like 'f'
	BEGIN
		UPDATE PERSON SET MID=NULL WHERE MID= @id;
	END
	ELSE BEGIN
		DECLARE @gen HIERARCHYID, @fid INT;
		SET @fid = (SELECT FAMILY_ID FROM PERSON WHERE ID= @id);
		SET @gen = (SELECT GEN FROM PERSON WHERE ID=@id);
		IF ((SELECT COUNT(*) FROM PERSON WHERE gen.IsDescendantOf(@gen)=1 and family_id=@fid and id != @id) > 0)
		BEGIN
			EXEC ChildrenHidChange @generation=@gen, @pid=@id;
		END
	END
	DELETE FROM PERSON WHERE ID=@id;
END

CREATE PROCEDURE ChildrenHidChange
	@generation HIERARCHYID, @pid INT
AS
	
BEGIN
	DECLARE @idc AS INT;
	DECLARE @genc as HIERARCHYID;
	DECLARE @fid AS INT;
	SET @fid = (SELECT family_id FROM PERSON WHERE ID=@pid);
	DECLARE CHILDREN_CURSOR CURSOR
	FOR
	SELECT id FROM PERSON 
	WHERE gen.IsDescendantOf(@generation)=1 and family_id=@fid and id != @pid AND (gen.GetLevel() - @generation.GetLevel() < 2)

	OPEN CHILDREN_CURSOR
	FETCH NEXT FROM CHILDREN_CURSOR INTO @idc;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @genc = (select gen from person where id=@idc);
		UPDATE PERSON SET family_id=@idc, gen='/' WHERE id=@idc;
		UPDATE PERSON SET family_id=@idc, gen=REPLACE(gen.ToString(), @genc.ToString(), '/') WHERE gen.IsDescendantOf(@genc)=1 and  family_id=@fid;
		FETCH NEXT FROM CHILDREN_CURSOR INTO @idc;
	END

	CLOSE CHILDREN_CURSOR
	DEALLOCATE CHILDREN_CURSOR
END

DROP PROCEDURE ChildrenHidChange;
DROP PROCEDURE RemovePerson;

CREATE PROCEDURE RemoveRelationship
	@idp INT, @idc INT
AS
BEGIN
	DECLARE @pgender AS VARCHAR(7);
	SET @pgender = (SELECT gender FROM PERSON WHERE ID = @idp);
	IF @pgender = 'f'
	BEGIN
		UPDATE PERSON SET MID=NULL WHERE ID=@idc and MID=@idp;
	END
	ELSE BEGIN
		DECLARE @pgen AS HIERARCHYID, @cgen AS HIERARCHYID, @cfid AS INT, @pfid AS INT;
		SET @pgen = (SELECT gen FROM PERSON WHERE id = @idp);
		SET @pfid = (SELECT family_id FROM PERSON WHERE id = @idp);
		SET @cgen = (SELECT gen FROM PERSON WHERE id = @idc);
		SET @cfid = (SELECT family_id FROM PERSON WHERE id = @idc);
		IF (@cgen.IsDescendantOf(@pgen) = 1) AND (@pgen.GetLevel() - @cgen.GetLevel() < 2 AND @cfid = @pfid)
		BEGIN
			IF ((SELECT COUNT(*) FROM PERSON WHERE gen.IsDescendantOf(@cgen)=1 and family_id=@pfid and id != @idc) > 0)
			BEGIN
				EXEC ChildrenHidChange @generation=@pgen, @pid=@idp;
			END
			UPDATE PERSON SET family_id = id, gen = '/' where id = @idc;
		END
	END
end

DROP PROCEDURE RemoveRelationship;

DROP TABLE PERSON;
 create Table Person
 (id INT IDENTITY(1,1) PRIMARY KEY,
 mid INT,
 family_id INT,
 firstname VARCHAR(50),
 lastname VARCHAR(50),
 dateofbirth DATE,
 gender VARCHAR(7),
 gen HIERARCHYID,
 FOREIGN KEY(mid) references Person(id)
 );


DROP PROCEDURE GetDescendants;


CREATE PROCEDURE GetDescendants
    @id INT
as
begin
	CREATE TABLE #DESCENDANTS
	( 
		id INT,
		firstname VARCHAR(50), 
		lastname VARCHAR(50),
		dateofbirth date, 
		gender VARCHAR(7),
		mid int, 
		gener HIERARCHYID, 
		fid INT
	);
	DECLARE @gen as HIERARCHYID, @idc AS INT;
	DECLARE @fid as INT, @gender AS VARCHAR(6);
	DECLARE @fatherid as INT, @fathershid HIERARCHYID;
	SET @gen = (SELECT GEN FROM PERSON WHERE ID= @id);
	SET @fid = (SELECT family_id FROM PERSON WHERE ID= @id);
	SET @gender = (SELECT gender FROM PERSON WHERE ID= @id);
	

	IF @gender LIKE 'f'
	BEGIN
		
		CREATE TABLE #child
		( 
		id INT
		);

		WITH CHILDREN (id)
		AS
		(
			SELECT id FROM PERSON WHERE mid = @id
		)
		INSERT INTO #child SELECT * FROM CHILDREN;

		
		DECLARE DESCENDANTS_CURSOR CURSOR
		FOR
		SELECT id from #child

		OPEN DESCENDANTS_CURSOR
		FETCH NEXT FROM DESCENDANTS_CURSOR INTO @idc;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @fid = (SELECT family_id FROM PERSON WHERE ID= @idc);
			SET @gen = (SELECT GEN FROM PERSON WHERE ID= @idc);
			SET @fathershid = (SELECT @gen.GetAncestor(1) FROM PERSON WHERE ID=@idc);
			SET @fatherid = (SELECT ID FROM PERSON WHERE gen = @fathershid and family_id = @fid);
			INSERT INTO #DESCENDANTS
			EXEC GetDescendants @id = @fatherid;

			FETCH NEXT FROM DESCENDANTS_CURSOR INTO @idc;
		END

		CLOSE DESCENDANTS_CURSOR
		DEALLOCATE DESCENDANTS_CURSOR
		drop table #child;
	END
	ELSE IF @gender LIKE 'm'
	BEGIN
		with maleLine (id, firstname, lastname, dateofbirth, gender, mid, gener, fid)
		AS	
		(
			select id, firstname, lastname, dateofbirth, gender, mid, gen.ToString(), family_id
			from person where gen.IsDescendantOf(@gen) = 1 and gen != @gen and family_id = @fid
		),
		femaleline (id, firstname, lastname, dateofbirth, gender, mid, gener, fid) 
		as
		(
			SELECT p.id, p.firstname, p.lastname, p.dateofbirth, p.gender, p.mid, p.gen.ToString(), family_id
			FROM person p join maleLine ml on ml.id = p.mid
			UNION
			select * from maleLine
		)
		INSERT INTO #DESCENDANTS SELECT * FROM femaleline;
	END

	SELECT * FROM #DESCENDANTS;
	DROP TABLE #DESCENDANTS;
end

DROP PROCEDURE GetDescendants;
EXEC GetDescendants @id = 5;
select *,  gen.ToString()from person;

CREATE PROCEDURE GetAncestors
	 @id INT
AS
BEGIN
	
	CREATE TABLE #ANC
	( 
		id INT,
		firstname VARCHAR(50), 
		lastname VARCHAR(50),
		dateofbirth date, 
		gender VARCHAR(7),
		n INT
	);

	DECLARE @gen as HIERARCHYID, @fid AS INT, @mid AS INT;
	SET @gen = (SELECT GEN FROM PERSON WHERE ID=@id);
	SET @fid = (SELECT FAMILY_ID FROM PERSON WHERE ID=@id);
	SET @mid = (SELECT mid FROM PERSON WHERE ID=@id);
    WITH Ancestors(gen, id, firstname, lastname, dateofbirth, gender, AncestorID, mid, family_id, n) AS
    (
        SELECT
            gen, id,  firstname, lastname, dateofbirth, gender, gen.GetAncestor(1), family_id, mid, 1
        FROM
            person
        WHERE gen = @gen AND FAMILY_ID = @fid

        UNION ALL

        SELECT
            p.gen, p.id, p.firstname, p.lastname, p.dateofbirth, p.gender, p.gen.GetAncestor(1), p.mid, p.family_id, n+1
        FROM
            person p
        INNER JOIN
            Ancestors a ON p.gen = a.AncestorID AND p.FAMILY_ID = @fid
    ),
	AncestorsMothers (gen, id, firstname, lastname, dateofbirth, gender, AncestorID, mid, family_id, n) AS
	(
		SELECT
            gen, id,  firstname, lastname, dateofbirth, gender, gen.GetAncestor(1), family_id, mid, 2
        FROM
            person
        WHERE id = @mid

        UNION ALL

        SELECT
            p.gen, p.id, p.firstname, p.lastname, p.dateofbirth, p.gender, p.gen.GetAncestor(1), p.mid, p.family_id, n+1
        FROM
            person p
        JOIN
            AncestorsMothers a ON p.id = a.mid
	)
	INSERT INTO #ANC ( id, firstname, lastname, dateofbirth, gender, n)
	SELECT  id, firstname, lastname, dateofbirth, gender, n FROM Ancestors
	union all
	select a.id, a.firstname, a.lastname, a.dateofbirth, a.gender, n from AncestorsMothers a join Person p on a.mid = p.id ;

	INSERT INTO #ANC ( id, firstname, lastname, dateofbirth, gender, n) SELECT  id, firstname, lastname, dateofbirth, gender, 2 FROM PERSON WHERE ID = @mid;
	SELECT DISTINCT * FROM #ANC WHERE id != @id;
	DROP TABLE #ANC;
END

DROP PROCEDURE GetAncestors;

EXEC GetAncestors @id = 12;
select *,  gen.ToString()from person;


-----------------------------------------
/*
CREATE PROCEDURE GetDescendants
    @id INT
as
begin
	DECLARE @gen as HIERARCHYID;
	DECLARE @fid as INT;
	SET @gen = (SELECT GEN FROM PERSON WHERE ID= @id);
	SET @fid = (SELECT family_id FROM PERSON WHERE ID= @id);
	with maleLine (id, firstname, lastname, dateofbirth, gender, mid, gener, fid)
	AS	(select id, firstname, lastname, dateofbirth, gender, mid, gen.ToString(), family_id from person where gen.IsDescendantOf(@gen) = 1 and gen != @gen)
	SELECT p.id, p.firstname, p.lastname, p.dateofbirth, p.gender, p.mid, p.gen.ToString() FROM person p join maleLine ml on ml.id = p.mid
	UNION
	select * from maleLine;
end



DROP PROCEDURE GetDescendants;
CREATE PROCEDURE GetDescendants
    @id INT
as
begin
	CREATE TABLE #DESC
	( 
		id INT,
		firstname VARCHAR(50), 
		lastname VARCHAR(50),
		dateofbirth date, 
		gender VARCHAR(7),
		n INT
	);

	with descen(id, fname, lname, dob, gender, n, fid, gen)
	as
	(
		SELECT id, firstname, lastname, dateofbirth, gender, 2, family_id, gen FROM PERSON 
		WHERE MID=@id

		UNION ALL

        SELECT p.id, p.firstname, p.lastname, p.dateofbirth, p.gender, n+1, p.family_id, p.gen
		from PERSON P JOIN descen D ON P.MID = D.ID
	),
	other(id, fname, lname, dob, gender, n, fid, gen)
	as
	(
		SELECT id, fname, lname, dob, gender, n, fid, gen from descen where gen is not null
		
		UNION ALL

		SELECT p.id, p.firstname, p.lastname, p.dateofbirth, p.gender, n+1, p.family_id, p.gen
		from PERSON P JOIN other O ON o.fid = p.family_id and p.gen.IsDescendantOf(o.gen) = 1
	)
	INSERT INTO #DESC ( id, firstname, lastname, dateofbirth, gender, n)
	SELECT  id, fname, lname, dob, gender, n FROM descen
	union all
	SELECT id, fname, lname, dob, gender, n FROM OTHER;

	SELECT DISTINCT * FROM #DESC WHERE id != @id;
	DROP TABLE #DESC;
end*/
/*
exec GetDescendants @id = 1;

	with descen(id, fname, lname, dob, gender, n, fid, gen)
	as
	(
		SELECT id, firstname, lastname, dateofbirth, gender, 2, family_id, gen FROM PERSON 
		WHERE MID=5

		UNION ALL

        SELECT p.id, p.firstname, p.lastname, p.dateofbirth, p.gender, n+1, p.family_id, p.gen
		from PERSON P JOIN descen D ON P.MID = D.ID
	)
	other(id, fname, lname, dob, gender, n, fid, gen)
	as
	(
		SELECT id, fname, lname, dob, gender, n, fid, gen from descen where gen is not null and gender LIKE 'm'
		
		UNION ALL

		SELECT p.id, p.firstname, p.lastname, p.dateofbirth, p.gender, n+1, p.family_id, p.gen
		from PERSON P JOIN other O ON o.fid = p.family_id and p.gen.IsDescendantOf(o.gen) = 1 and p.id != 5
	) 	SELECT  id, fname, lname, dob, gender, n FROM descen
	union all
	SELECT id, fname, lname, dob, gender, n FROM OTHER;*/






--testing

EXEC AddPerson @fname = 'dziadek', @lname ='ktos', @dob= '1900-07-27', @gender='m';
EXEC AddPerson @fname = 'syn', @lname ='ktos', @dob= '1930-07-27', @gender='m';
EXEC AddPerson @fname = 'wnuk', @lname ='ktos', @dob= '1960-07-27', @gender='f';
EXEC AddPerson @fname = 'wnuk2', @lname ='ktos', @dob= '1962-07-27', @gender='f';
EXEC AddPerson @fname = 'corka', @lname ='inna', @dob= '1935-07-27', @gender='f';
EXEC AddPerson @fname = 'maz corki', @lname ='inna', @dob= '1928-07-27', @gender='m';
EXEC AddPerson @fname = 'corka corki', @lname ='inna', @dob= '1957-07-27', @gender='f';
EXEC AddPerson @fname = 'prawnuczka z rodu corki', @lname ='inna', @dob= '1989-07-27', @gender='f';

select * , gen.ToString() from person;
EXEC AddParent @idp = 2, @idc = 3;
EXEC AddParent @idp = 2, @idc = 4;
EXEC AddParent @idp = 1, @idc = 2;
EXEC AddParent @idp = 1, @idc = 5;
EXEC AddParent @idp = 5, @idc = 7;
EXEC AddParent @idp = 6, @idc = 7;

select * , gen.ToString() from person;
EXEC AddParent @idp = 7, @idc = 8;
select * , gen.ToString() from person;
delete from person;

EXEC GetAncestors @id =3;


EXEC RemovePerson @id = 1;
select *,  gen.ToString()from person;

EXEC RemoveRelationship @idp=2, @idc=3;
select *,  gen.ToString()from person;