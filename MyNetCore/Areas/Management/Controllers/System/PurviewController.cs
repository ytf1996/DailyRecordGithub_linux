using MyNetCore.Controllers;
using MyNetCore.Models;
using Roim.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;

namespace MyNetCore.Areas.Management.Controllers
{
    [Area(areaName: "Management")]
    public class PurviewController : CommonController
    {
        public IActionResult SetTerritory()
        {
            BusinessPurview businessPur = new BusinessPurview();

            Users currentUser = businessPur.GetCurrentUserInfo();

            if(currentUser == null || !currentUser.IsAdmin)
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", MessageText.ErrorInfo.您无此操作权限.ToString()));
            }

            List<Purview> listPurview = new List<Purview>();

            string territoryProfilesIdStr = Request.Query["territoryProfilesId"];
            int territoryProfilesIdInt = 0;

            if (string.IsNullOrWhiteSpace(territoryProfilesIdStr) || !int.TryParse(territoryProfilesIdStr, out territoryProfilesIdInt))
            {
                return View(listPurview);
            }

            var result = businessPur.GetList(m => m.TerritoryProfilesId == territoryProfilesIdInt);

            if (result != null)
            {
                listPurview = result.ToList();
            }

            TerritoryProfiles modelTerritoryProfiles = BusinessHelper.ListTerritoryProfiles.FirstOrDefault(m => m.Id == territoryProfilesIdInt);
            ViewBag.modelTerritoryProfiles = modelTerritoryProfiles;

            return View(listPurview);
        }

        [HttpPost]
        public IActionResult SetTerritoryAction(List<int> Id, List<int> CreatedById, List<DateTime> CreatedDate, List<string> FullName,
            List<PurviewType> PurviewType, List<int> TerritoryProfilesId, List<int?> OtherTerritoryId
            , string CanInsert, string CanSelect, string CanUpdate, string CanDelete)
        {
            if (Id == null || Id.Count == 0
                || CreatedById == null || CreatedById.Count == 0
                || CreatedDate == null || CreatedDate.Count == 0
                || FullName == null || FullName.Count == 0
                || PurviewType == null || PurviewType.Count == 0
                || TerritoryProfilesId == null || TerritoryProfilesId.Count == 0
                || OtherTerritoryId == null || OtherTerritoryId.Count == 0
                || string.IsNullOrWhiteSpace(CanInsert)
                || string.IsNullOrWhiteSpace(CanSelect)
                || string.IsNullOrWhiteSpace(CanUpdate)
                || string.IsNullOrWhiteSpace(CanDelete)
                )
            {
                return Failure("数据提交不成功，操作失败");
            }

            string[] CanInserts = CanInsert.Split(',');
            string[] CanSelects = CanSelect.Split(',');
            string[] CanUpdates = CanUpdate.Split(',');
            string[] CanDeletes = CanDelete.Split(',');

            if (Id.Count != CreatedById.Count
                || Id.Count != CreatedById.Count
                || Id.Count != CreatedDate.Count
                || Id.Count != FullName.Count
                || Id.Count != PurviewType.Count
                || Id.Count != TerritoryProfilesId.Count
                || Id.Count != OtherTerritoryId.Count
                || Id.Count != CanInserts.Length
                || Id.Count != CanSelects.Length
                || Id.Count != CanUpdates.Length
                || Id.Count != CanDeletes.Length)
            {
                return Failure("数据提交不完全，操作失败");
            }

            List<Purview> list = new List<Purview>();

            if (TerritoryProfilesId == null || TerritoryProfilesId.Count == 0 || TerritoryProfilesId.Distinct().Count() > 1)
            {
                return Failure("用户信息错误，操作失败");
            }

            int territoryProfilesId = TerritoryProfilesId[0];

            if (territoryProfilesId == 0)
            {
                return Failure("用户信息错误，操作失败");
            }

            for (int i = 0; i < Id.Count; i++)
            {
                Purview model = new Purview();
                model.Id = Id[i];
                model.CreatedById = CreatedById[i];
                model.CreatedDate = CreatedDate[i];
                model.FullName = FullName[i];
                model.PurviewType = PurviewType[i];
                model.TerritoryProfilesId = territoryProfilesId;
                model.OtherTerritoryId = OtherTerritoryId[i];
                model.CanInsert = CanInserts[i] == "true";
                model.CanSelect = CanSelects[i] == "true";
                model.CanUpdate = CanUpdates[i] == "true";
                model.CanDelete = CanDeletes[i] == "true";
                list.Add(model);
            }

            BusinessPurview businessPur = new BusinessPurview();
            businessPur.Update(list, territoryProfilesId);

            return Success("操作成功", null, "/Management/TerritoryProfiles/List");
        }

    }
}