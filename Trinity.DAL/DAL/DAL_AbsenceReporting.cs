using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;
using Trinity.Common;

namespace Trinity.DAL
{
    public class DAL_AbsenceReporting
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public void InsertAbsentReason(List<Dictionary<string, string>> dataUpdate)
        {
            List<Guid> ArrayIDAppointments = dataUpdate.Select(d => new Guid(d["ID"])).ToList();
            var arrayUpdate = _localUnitOfWork.DataContext.Appointments.Where(d => ArrayIDAppointments.Contains(d.ID) && !d.AbsenceReporting_ID.HasValue).ToList();
            List<Trinity.DAL.DBContext.AbsenceReporting> arrayInssert = new List<Trinity.DAL.DBContext.AbsenceReporting>();
            foreach (var item in arrayUpdate)
            {

                var data = dataUpdate.FirstOrDefault(d => new Guid(d["ID"]) == item.ID);

                Trinity.DAL.DBContext.AbsenceReporting dataAbsence = new Trinity.DAL.DBContext.AbsenceReporting()
                {
                    AbsenceReason = short.Parse(data["ChoseNumber"]),
                    ID = Guid.NewGuid(),
                    ReportingDate = DateTime.Now
                };
                item.AbsenceReporting_ID = dataAbsence.ID;
                arrayInssert.Add(dataAbsence);
                //if (dataAbsence.AbsenceReason == (short)EnumAbsentReasons.No_Supporting_Document)
                //    Lib.SignalR..SendToAllDutyOfficers(item.UserId, "Supervisee get queue without supporting document", "Please check the Supervisee's information!", EnumNotificationTypes.Notification);
            }
            _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.AbsenceReporting>().AddRange(arrayInssert);
            foreach (var item in arrayUpdate)
            {
                _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Appointment>().Update(item);
            }
            _localUnitOfWork.Save();
        }
        public List<AbsenceReporting> GetByUserID(string userId)
        {
            return _localUnitOfWork.DataContext.AbsenceReportings.Include("Appointments").Where(d => d.Appointments.Any(c => c.UserId == userId)).ToList();
        }

        public int SaveAbsendReporing(AbsenceReporting absenceReporting)
        {
            _localUnitOfWork.GetRepository<AbsenceReporting>().Update(absenceReporting);
            return _localUnitOfWork.Save();
        }

        public bool CreateAbsenceReporting(Trinity.BE.AbsenceReporting model, bool isLocal)
        {
            try
            {
                var absenceRepo = _localUnitOfWork.GetRepository<AbsenceReporting>();
                AbsenceReporting dbAbsence = SetInfo(model);
                absenceRepo.Add(dbAbsence);
                _localUnitOfWork.Save();
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
            //dbAbsence.ScannedDocument = model.ScannedDocument;
            dbAbsence.AbsenceReason = model.AbsenceReason;
            return dbAbsence;
        }


        /// <summary>
        /// Set new info and reason
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public BE.AbsenceReporting SetInfo(Trinity.BE.Reason reason)
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
