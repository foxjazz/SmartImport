USE [Service]
GO

/****** Object:  StoredProcedure [SSIS].[TempoComtags_UpdateInsert]    Script Date: 7/11/2017 11:05:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE proc [SSIS].[TempoComtags_UpdateInsert]
as
begin
set nocount on
begin try
begin tran

insert into importdata.Tempo.Comtags_Backup (ComtagsID,AccountName,LoanNo,ReferralID,ComtagGroupDescription,ComtagNumber,ComtagDescription,ComtagStartDate,
ComtagProjectedDate,ComtagCompletedDate,Active,ComtagResponseBy,ComtagCreatedBy,ComtagModifiedBy,ComtagClosedBy,ArchiveCreatedBy,ArchiveCreatedDate,
ArchiveStatus,IssueDetailID,RetireReason,ComtagCommentMadeBy,ComtagComment,LastModifiedDate,ResponseType,LastResponseTypeChangeDate,AssignedTo,
ComtagTransferredTo,LastTransferDate,LastEmailNotificationSentTo,LastEmailNotificationDate,ReferralType,ReferralSubType,DateImported,SourceFilename) 

select SRC.ComtagsID, SRC.AccountName, SRC.LoanNo, SRC.ReferralID, SRC.ComtagGroupDescription, SRC.ComtagNumber, SRC.ComtagDescription, SRC.ComtagStartDate, 
SRC.ComtagProjectedDate, SRC.ComtagCompletedDate, SRC.Active, SRC.ComtagResponseBy, SRC.ComtagCreatedBy, SRC.ComtagModifiedBy, SRC.ComtagClosedBy, 
SRC.ArchiveCreatedBy, SRC.ArchiveCreatedDate, SRC.ArchiveStatus, SRC.IssueDetailID, SRC.RetireReason, SRC.ComtagCommentMadeBy, SRC.ComtagComment, 
SRC.LastModifiedDate, SRC.ResponseType, SRC.LastResponseTypeChangeDate, SRC.AssignedTo, SRC.ComtagTransferredTo, SRC.LastTransferDate, SRC.LastEmailNotificationSentTo, 
SRC.LastEmailNotificationDate, SRC.ReferralType, SRC.ReferralSubType, SRC.DateImported, SRC.SourceFilename 
from importdata.tempo.ComtagsSI SRC 


  select  max(SRC.LastModifiedDate) d, SRC.referralid, SRC.comtagnumber into #p
from importdata.tempo.comtagsSI SRC inner join sb.tempocomtags T on SRC.referralid = T.referralid and SRC.comtagnumber = T.comtagnumber  
group by src.referralid, src.comtagnumber

update SB.Tempocomtags set AccountName = I.AccountName,LoanNo = I.LoanNo,ReferralID = I.ReferralID,
ComtagGroupDescription = I.ComtagGroupDescription,ComtagNumber = I.ComtagNumber,ComtagDescription = I.ComtagDescription,ComtagStartDate = I.ComtagStartDate,
ComtagProjectedDate = I.ComtagProjectedDate,ComtagCompletedDate = I.ComtagCompletedDate,Active = I.Active,ComtagResponseBy = I.ComtagResponseBy,
ComtagCreatedBy = I.ComtagCreatedBy,ComtagModifiedBy = I.ComtagModifiedBy,ComtagClosedBy = I.ComtagClosedBy,ArchiveCreatedBy = I.ArchiveCreatedBy,
ArchiveCreatedDate = I.ArchiveCreatedDate,ArchiveStatus = I.ArchiveStatus,IssueDetailID = I.IssueDetailID,RetireReason = I.RetireReason,
ComtagCommentMadeBy = I.ComtagCommentMadeBy,ComtagComment = I.ComtagComment,LastModifiedDate = I.LastModifiedDate,ResponseType = I.ResponseType,
LastResponseTypeChangeDate = I.LastResponseTypeChangeDate,AssignedTo = I.AssignedTo,ComtagTransferredTo = I.ComtagTransferredTo,
LastTransferDate = I.LastTransferDate,LastEmailNotificationSentTo = I.LastEmailNotificationSentTo,LastEmailNotificationDate = I.LastEmailNotificationDate,
ReferralType = I.ReferralType,ReferralSubType = I.ReferralSubType,DateImported = I.DateImported,SourceFilename = I.SourceFilename
from importdata.tempo.comtagsSI I inner join SB.Tempocomtags U on I.referralid = U.referralid and I.comtagnumber = U.comtagnumber
inner join #p on U.referralid = #p.referralid and U.comtagnumber = #p.comtagnumber and I.lastmodifieddate = #p.d


drop table #p
delete importdata.tempo.comtagssi from importdata.tempo.comtagsSI U 
inner join SB.tempocomtags S on U.referralid = S.referralid and U.comtagnumber = S.comtagnumber;

  select  max(SRC.LastModifiedDate) d,  SRC.referralid, SRC.comtagnumber into #t
from importdata.tempo.comtagsSI SRC group by src.referralid, src.comtagnumber;


WITH cte
     AS (SELECT ROW_NUMBER() OVER (PARTITION BY SRC.ReferralID,  SRC.ComtagNumber, LastModifiedDate 
                                       ORDER BY ( SELECT comtagsid)) RN
         FROM   importdata.tempo.ComtagsSI SRC)
DELETE FROM cte
WHERE  RN > 1;



insert into SB.TempoComtags (TempoComtagsID,AccountName,LoanNo,ReferralID,ComtagGroupDescription,ComtagNumber,ComtagDescription,ComtagStartDate,
ComtagProjectedDate,ComtagCompletedDate,Active,ComtagResponseBy,ComtagCreatedBy,ComtagModifiedBy,ComtagClosedBy,ArchiveCreatedBy,ArchiveCreatedDate,
ArchiveStatus,IssueDetailID,RetireReason,ComtagCommentMadeBy,ComtagComment,LastModifiedDate,ResponseType,LastResponseTypeChangeDate,AssignedTo,
ComtagTransferredTo,LastTransferDate,LastEmailNotificationSentTo,LastEmailNotificationDate,ReferralType,ReferralSubType,DateImported,SourceFilename) 
select SRC.ComtagsID, SRC.AccountName, SRC.LoanNo, SRC.ReferralID, SRC.ComtagGroupDescription, SRC.ComtagNumber, SRC.ComtagDescription, SRC.ComtagStartDate, 
SRC.ComtagProjectedDate, SRC.ComtagCompletedDate, SRC.Active, SRC.ComtagResponseBy, SRC.ComtagCreatedBy, SRC.ComtagModifiedBy, SRC.ComtagClosedBy, 
SRC.ArchiveCreatedBy, SRC.ArchiveCreatedDate, SRC.ArchiveStatus, SRC.IssueDetailID, SRC.RetireReason, SRC.ComtagCommentMadeBy, SRC.ComtagComment, 
SRC.LastModifiedDate, SRC.ResponseType, SRC.LastResponseTypeChangeDate, SRC.AssignedTo, SRC.ComtagTransferredTo, SRC.LastTransferDate, SRC.LastEmailNotificationSentTo, 
SRC.LastEmailNotificationDate, SRC.ReferralType, SRC.ReferralSubType, SRC.DateImported, SRC.SourceFilename 
from importdata.tempo.ComtagsSI SRC inner join #t on SRC.referralid = #t.referralid and SRC.comtagnumber = #t.comtagnumber and SRC.lastmodifieddate = #t.d 



drop table #t

delete importdata.tempo.comtagssi

commit
end try
BEGIN CATCH
	IF @@TRANCOUNT > 0
		ROLLBACK TRANSACTION;
	EXEC dbo.SQLErrorHandler;
	RETURN 55555;
END CATCH;
end


GO


