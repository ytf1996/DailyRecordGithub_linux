using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNetCore.Models;
using System.Reflection;

namespace MyNetCore.Business.Param
{
    public class BusinessSystemParam : BaseBusiness<SystemParam>
    {
        public override bool NeedCheckNameRepeat
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 排除的字段
        /// </summary>
        private string[] _PaiChuProps = new string[] { "ParamType", "MyConfig", "CreatedById" , "Name", "TerritoryId", "Id" , "CreatedBy", "CreatedDate", "UpdatedBy", "UpdatedById", "UpdatedDate", "Deleted" };

        public void Save<T>(T model) where T : BaseParam
        {
            if (model == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.数据提交失败);
            }

            var type = typeof(T);

            PropertyInfo[] props = type.GetProperties();

            if (props == null || !props.Any())
            {
                ThrowErrorInfo(MessageText.ErrorInfo.数据提交失败);
            }
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }

            if (!currentUser.IsAdmin)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }

            SystemParam modelSystemParam = null;

            foreach (var itemProp in props)
            {
                if (_PaiChuProps.Contains(itemProp.Name))
                {
                    continue;
                }

                modelSystemParam = GetByCondition(m => m.ParamType == model.ParamType && m.Name == itemProp.Name, false);
                if (modelSystemParam == null)
                {
                    modelSystemParam = new SystemParam();
                    modelSystemParam.CreatedById = currentUser.Id;
                    modelSystemParam.CreatedDate = DateTime.Now;
                    modelSystemParam.UpdatedById = currentUser.Id;
                    modelSystemParam.UpdatedDate = DateTime.Now;
                    modelSystemParam.ParamType = model.ParamType;
                    modelSystemParam.ParamValue = Roim.Common.ConvertHelper.ConvertToString(itemProp.GetValue(model));
                    modelSystemParam.Name = itemProp.Name;
                    Add(modelSystemParam, false);
                }
                else
                {
                    modelSystemParam.UpdatedById = currentUser.Id;
                    modelSystemParam.UpdatedDate = DateTime.Now;
                    modelSystemParam.ParamValue = Roim.Common.ConvertHelper.ConvertToString(itemProp.GetValue(model));
                    Edit(modelSystemParam, false);
                }
            }

        }


        public T Get<T>(bool needRight = false) where T : BaseParam
        {
            T model = System.Activator.CreateInstance<T>();

            if (needRight)
            {
                var currentUser = GetCurrentUserInfo();
                if (currentUser == null)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                }

                if (!currentUser.IsAdmin)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                }
            }

            var list = GetListByCondition(m => m.ParamType == model.ParamType, false);

            if (list != null && list.Any())
            {
                foreach (var itemProp in typeof(T).GetProperties())
                {
                    if (_PaiChuProps.Contains(itemProp.Name))
                    {
                        continue;
                    }

                    foreach (var itemModel in list)
                    {
                        if (itemModel.Name == itemProp.Name)
                        {
                            //itemProp.SetValue(model, itemModel.ParamValue);
                            if (!itemProp.PropertyType.IsGenericType)
                            {
                                try
                                {
                                    //非泛型
                                    itemProp.SetValue(model, string.IsNullOrEmpty(itemModel.ParamValue) ? null : Convert.ChangeType(itemModel.ParamValue, itemProp.PropertyType), null);
                                }
                                catch
                                {
                                    itemProp.SetValue(model, null);
                                }

                            }
                            else
                            {
                                try
                                {
                                    //泛型Nullable<>
                                    Type genericTypeDefinition = itemProp.PropertyType.GetGenericTypeDefinition();
                                    if (genericTypeDefinition == typeof(Nullable<>))
                                    {
                                        itemProp.SetValue(model, string.IsNullOrEmpty(itemModel.ParamValue) ? null : Convert.ChangeType(itemModel.ParamValue, Nullable.GetUnderlyingType(itemProp.PropertyType)), null);
                                    }
                                }
                                catch
                                {
                                    itemProp.SetValue(model, null);
                                }

                            }
                            break;
                        }
                    }
                }
            }

            return model;
        }

    }
}