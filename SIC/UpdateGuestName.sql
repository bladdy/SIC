INSERT INTO InvitationGuest (GuestName, GuestType, InvitationId, Status)
SELECT 
    'Adulto',
    1,
    i.Id,
    1
FROM Invitations i
INNER JOIN Events e ON e.Id = i.EventId
CROSS APPLY (
    SELECT TOP (i.NumberAdults)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.objects
) nums
WHERE e.Code = 'KHAXD7'
AND (
    SELECT COUNT(*) 
    FROM InvitationGuest ig 
    WHERE ig.InvitationId = i.Id AND ig.GuestType = 1
) < i.NumberAdults;
INSERT INTO InvitationGuest (GuestName, GuestType, InvitationId, Status)
SELECT 
    'Joven',
    2,
    i.Id,
    1
FROM Invitations i
INNER JOIN Events e ON e.Id = i.EventId
CROSS APPLY (
    SELECT TOP (i.NumberYouths)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.objects
) nums
WHERE e.Code = 'KHAXD7'
AND (
    SELECT COUNT(*) 
    FROM InvitationGuest ig 
    WHERE ig.InvitationId = i.Id AND ig.GuestType = 2
) < i.NumberYouths;

INSERT INTO InvitationGuest (GuestName, GuestType, InvitationId, Status)
SELECT 
    'Niño',
    3,
    i.Id,
    1
FROM Invitations i
INNER JOIN Events e ON e.Id = i.EventId
CROSS APPLY (
    SELECT TOP (i.NumberChildren)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.objects
) nums
WHERE e.Code = 'KHAXD7'
AND (
    SELECT COUNT(*) 
    FROM InvitationGuest ig 
    WHERE ig.InvitationId = i.Id AND ig.GuestType = 3
) < i.NumberChildren;

/*Acutaliza el GuestName*/
UPDATE ig
SET ig.GuestName = 
    CASE ig.GuestType
        WHEN 1 THEN 'Adulto'
        WHEN 2 THEN 'Joven'
        WHEN 3 THEN 'Niño'
    END
 FROM InvitationGuest ig
INNER JOIN Invitations i ON i.Id = ig.InvitationId
INNER JOIN Events e ON e.Id = i.EventId
WHERE 
    e.Code = 'KHAXD7'
    AND (ig.GuestName IS NULL OR LTRIM(RTRIM(ig.GuestName)) = '')
    AND ig.GuestType IN (1, 2, 3);


SELECT ig.Id, ig.GuestType, ig.GuestName, e.Code
FROM InvitationGuest ig
INNER JOIN Invitations i ON i.Id = ig.InvitationId
INNER JOIN Events e ON e.Id = i.EventId
WHERE 
    e.Code = 'KHAXD7'
    AND (ig.GuestName IS NULL OR LTRIM(RTRIM(ig.GuestName)) = '')
    AND ig.GuestType IN (1, 2, 3);


select * from InvitationGuest where Status = 1

update InvitationGuest
SET  Status = 2 
where Status = 1

select * from dbo.Events where Code ='KHAXD7'

;WITH Adultos AS (
    SELECT
        ig.Id,
        ig.InvitationId,
        ROW_NUMBER() OVER (PARTITION BY ig.InvitationId ORDER BY ig.Id) AS rn,
        i.NumberConfirmedAdults,
        i.Status AS InvitationStatus
		--ig.Status
    FROM InvitationGuest ig
    INNER JOIN Invitations i 
        ON i.Id = ig.InvitationId
    INNER JOIN Events e
        ON e.Id = i.EventId
    WHERE ig.GuestType = 1
	AND i.Code = '6URAR6'
      AND i.EventId = 28006
	  AND ig.Status != i.Status
      AND i.Status IN (2,19, 20)
      AND e.Status = 0   -- ?? SOLO eventos activos

)
UPDATE ig
SET ig.Status = CASE
					WHEN a.InvitationStatus = 2 THEN 2
                    WHEN a.InvitationStatus = 20 THEN 20
                    WHEN a.rn <= a.NumberConfirmedAdults THEN 19
                    ELSE 20
                END
FROM InvitationGuest ig
INNER JOIN Adultos a 
    ON ig.Id = a.Id;