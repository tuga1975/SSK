using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using Trinity.Common;

namespace Trinity.BackendAPI.Controllers
{

    public class APSBookAppointmentResponses
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }


    [Route("api/APS/{Action}")]
    public class APSController : ApiController
    {
        [HttpPost]
        [ResponseType(typeof(APSBookAppointmentResponses))]
        public async System.Threading.Tasks.Task<IHttpActionResult> APSBookAppointment(string UserID, string AppointmentID, string TimeSlotID)
        {
            try
            {
                var user = new DAL.DAL_Membership_Users().GetByUserId(UserID);
                if (user != null)
                {
                    var appointment = new DAL.DAL_Appointments().GetAppointmentByID(Guid.Parse(AppointmentID));
                    if (appointment != null)
                    {
                        var timeslot = new DAL.DAL_Timeslots().GetByID(TimeSlotID);
                        if (timeslot != null)
                        {
                            if (appointment.ChangedCount < 3)
                            {
                                if (new DAL.DAL_Appointments().CountApptmtHasUseByTimeslot(timeslot.Timeslot_ID) < timeslot.MaximumSupervisee.Value)
                                {
                                    if (!new DAL.DAL_QueueNumber().IsInQueue(AppointmentID, EnumStation.SSK))
                                    {
                                        bool updateResult = new DAL.DAL_Appointments().UpdateTimeslot_ID(AppointmentID, timeslot.Timeslot_ID);
                                        if (updateResult)
                                        {
                                            await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.AppointmentBooked(UserID,AppointmentID,TimeSlotID,EnumStation.APS));
                                            return Ok(new APSBookAppointmentResponses()
                                            {
                                                Status = true,
                                                Message = "Success"
                                            });
                                        }
                                        else
                                        {
                                            return Ok(new APSBookAppointmentResponses()
                                            {
                                                Status = false,
                                                Message = "Booking Appointment failed. Please try again"
                                            });
                                        }
                                        
                                    }
                                    else
                                    {
                                        return Ok(new APSBookAppointmentResponses()
                                        {
                                            Status = false,
                                            Message = "User have already queued! The timeslot cannot be changed."
                                        });
                                    }
                                }
                                else
                                {
                                    return Ok(new APSBookAppointmentResponses()
                                    {
                                        Status = false,
                                        Message = "Timeslot too many people allowed"
                                    });

                                }
                            }
                            else
                            {
                                return Ok(new APSBookAppointmentResponses()
                                {
                                    Status = false,
                                    Message = "You have exceeded the number of times allowed for time slot change"
                                });
                            }
                        }
                        else
                        {
                            return Ok(new APSBookAppointmentResponses()
                            {
                                Status = false,
                                Message = "Timeslot not found"
                            });
                        }
                    }
                    else
                    {
                        return Ok(new APSBookAppointmentResponses()
                        {
                            Status = false,
                            Message = "Appointment not found"
                        });
                    }
                }
                else
                {
                    return Ok(new APSBookAppointmentResponses()
                    {
                        Status = false,
                        Message = "User not found"
                    });
                }
            }
            catch (Exception ex)
            {

                return Ok(new APSBookAppointmentResponses()
                {
                    Status = false,
                    Message = ex.Message
                });
            }

        }

    }
}
