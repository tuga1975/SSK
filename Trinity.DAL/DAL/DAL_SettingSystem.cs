using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_SettingSystem
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public string GenerateMarkingNumber()
        {
            string result = string.Empty;
            int currentYear = DateTime.Now.Year;
            Setting settingSystem = GetSettingSystemByYear(currentYear);

            if (settingSystem != null)
            {
                settingSystem.MaxMarkingNo += 1;
            }
            else
            {
                settingSystem = new Setting();
                settingSystem.Year = currentYear;
                settingSystem.MaxMarkingNo = 1;
            }

            result = "CSA" + (settingSystem.Year % 100).ToString() + settingSystem.MaxMarkingNo.ToString().PadLeft(6, '0');

            UpdateSettingSystem(settingSystem);

            return result;
        }

        public Setting GetSettingSystemByYear(int year)
        {
            // Must get Setting from Centralize to make sure MarkingNo is latest
            //if (EnumAppConfig.IsLocal)
            //{
            //    var data = _localUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Year == year);

            //    if (data != null)
            //    {
            //        return data;
            //    }
            //    else
            //    {
            //        bool centralizeStatus;
            //        var centralData = CallCentralized.Get<DBContext.Setting>(EnumAPIParam.SettingSystem, EnumAPIParam.GetSettingSystemByYear, out centralizeStatus, "year=" + year.ToString());
            //        if (centralizeStatus)
            //        {
            //            return centralData;
            //        }
            //        return null;
            //    }
            //}
            //else
            //{
            if (EnumAppConfig.ByPassCentralizedDB)
            {
                return _localUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Year == year); ;
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Year == year);
            }
            //}
        }

        public Setting UpdateSettingSystem(Setting setting)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    Setting dbLocalSetting = _localUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Year == setting.Year);

                    if (dbLocalSetting == null)
                    {
                        _localUnitOfWork.GetRepository<Setting>().Add(setting);
                    }
                    else
                    {
                        _localUnitOfWork.GetRepository<Setting>().Update(setting);
                    }

                    _localUnitOfWork.Save();
                    
                    if (EnumAppConfig.ByPassCentralizedDB)
                    {
                        return setting;
                    }
                    else
                    {
                        bool centralizeStatus = false;
                        var centralUpdate = CallCentralized.Post<Setting>(EnumAPIParam.SettingSystem, EnumAPIParam.UpdateSettingSystem, out centralizeStatus, setting);
                        if (centralizeStatus)
                        {
                            return centralUpdate;
                        }
                        else
                        {
                            throw new Exception(EnumMessage.NotConnectCentralized);
                        }
                    }                    
                }
                else
                {
                    Setting dbCentralSetting = _centralizedUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Year == setting.Year);
                    if (dbCentralSetting == null)
                    {
                        _centralizedUnitOfWork.GetRepository<Setting>().Add(setting);
                    }
                    else
                    {
                        _centralizedUnitOfWork.GetRepository<Setting>().Update(setting);
                    }
                    _centralizedUnitOfWork.Save();

                    return setting;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
