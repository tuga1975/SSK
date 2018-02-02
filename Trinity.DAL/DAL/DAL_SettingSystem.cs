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

            if(settingSystem != null)
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
            return _localUnitOfWork.DataContext.Settings.FirstOrDefault(s => s.Year == year);
        }

        public void UpdateSettingSystem(Setting setting)
        {
            try
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

                if (_centralizedUnitOfWork.Save() > 0)
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
                }
            }
            catch(Exception e)
            {

            }
        }
    }
}
