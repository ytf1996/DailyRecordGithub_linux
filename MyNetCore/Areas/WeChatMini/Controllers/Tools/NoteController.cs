using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using MyNetCore.Models;
using System;
using System.Linq;

namespace MyNetCore.Areas.WeChatMini.Controllers
{
    public class NoteController : WeChatMiniBaseWithAuthController  
    {
        /// <summary>
        /// 获取记事本列表
        /// </summary>
        /// <returns></returns>
        public IActionResult List()
        {
            BusinessNote businessNote = new BusinessNote();

            var list = businessNote.GetList(1, 100, out int totalCount, null, "UpdatedDate", true);

            var result = list.ToList();

            return Success(null, result);
        }

        /// <summary>
        /// 添加记事本
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public IActionResult Add(Note note)
        {
            if (string.IsNullOrWhiteSpace(note?.Remark))
            {
                return Failure("请填写内容");
            }
            if (string.IsNullOrWhiteSpace(note.Name))
            {
                note.Name = note.Remark.Split(Convert.ToChar(10))[0];
            }

            BusinessNote businessNote = new BusinessNote();
            businessNote.Add(note);
            return Success();
        }

        /// <summary>
        /// 获取单条记事本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Get(int id)
        {
            BusinessNote businessNote = new BusinessNote();
            var note = businessNote.GetById(id);
            return Success(data: note);
        }

        /// <summary>
        /// 修改记事本
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public IActionResult Edit(Note note)
        {
            if (string.IsNullOrWhiteSpace(note?.Remark))
            {
                return Failure("请填写内容");
            }
            if (string.IsNullOrWhiteSpace(note.Name))
            {
                note.Name = note.Remark.Split(Convert.ToChar(10))[0];
            }

            BusinessNote businessNote = new BusinessNote();
            var noteDB = businessNote.GetById(note.Id);
            noteDB.Name = note.Name;
            noteDB.Remark = note.Remark;
            businessNote.Edit(noteDB);
            return Success();
        }

        /// <summary>
        /// 删除记事本
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            BusinessNote businessNote = new BusinessNote();
            businessNote.DeleteById(id);
            return Success();
        }
    }
}
