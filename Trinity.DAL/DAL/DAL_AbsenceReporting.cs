using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_AbsenceReporting
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public List<AbsenceReporting> GetByUserID(string userId)
        {
            return _localUnitOfWork.DataContext.AbsenceReportings.Include("Appointments").Where(d => d.Appointments.Any(c => c.UserId == userId)).ToList();
        }

        public int SaveAbsendReporing(AbsenceReporting absenceReporting)
        {
            _localUnitOfWork.GetRepository<AbsenceReporting>().Update(absenceReporting);
            return _localUnitOfWork.Save();
        }

        public bool CreateAbsenceReporting(Trinity.BE.AbsenceReporting model,bool isLocal)
        {
            try
            {
                if (isLocal)
                {
                    var absenceRepo = _localUnitOfWork.GetRepository<AbsenceReporting>();
                    AbsenceReporting dbAbsence = SetInfo(model);
                    absenceRepo.Add(dbAbsence);
                    _localUnitOfWork.Save();

                }
                else
                {
                    var absenceRepo = _centralizedUnitOfWork.GetRepository<AbsenceReporting>();
                    AbsenceReporting dbAbsence = SetInfo(model);
                    absenceRepo.Add(dbAbsence);
                    _centralizedUnitOfWork.Save();

                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        /// <summary>
        /// SetInfo using model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static AbsenceReporting SetInfo(BE.AbsenceReporting model)
        {
            var dbAbsence = new AbsenceReporting();
            dbAbsence.ID = model.ID;
            dbAbsence.ReportingDate = model.ReportingDate;
            dbAbsence.ReasonDetails = model.ReasonDetails;
            dbAbsence.ScannedDocument = model.ScannedDocument;
            dbAbsence.AbsenceReason = model.AbsenceReason;
            return dbAbsence;
        }
      

        /// <summary>
        /// Set new info and reason
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public Trinity.BE.AbsenceReporting SetInfo(Trinity.BE.Reason reason)
        {
            var absenceModel = new Trinity.BE.AbsenceReporting();
            absenceModel.ID = Guid.NewGuid();
            absenceModel.AbsenceReason = reason.Value;
            absenceModel.ReasonDetails = reason.Detail;
            // set after scan document
            absenceModel.ScannedDocument = null;
            absenceModel.ReportingDate = DateTime.Now;

            return absenceModel;
        }
    }
}
