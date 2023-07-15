//日期转换格式
function ChangeDateFormat(begintime) {
    try {
        begintime = new Date(begintime);
        return begintime.getFullYear() + "/" + (parseInt(begintime.getMonth()) + 1) + "/" + begintime.getDate();
    }
    catch (e) {
        return "";
    }
}

//补零
function addPreZero(num) { var t = (num + '').length, s = ''; for (var i = 0; i < 2 - t; i++) { s += '0'; } return s + num; }

//判断是否包含中文
function hasChinaText(str) {
    if (escape(str).indexOf("%u") < 0) {
        return false;
    } else {
        return true;
    }
}

//获得网站域名url
function getRootPath() {
    var strFullPath = window.document.location.href;
    var strPath = window.document.location.pathname;
    var pos = strFullPath.indexOf(strPath);
    var prePath = strFullPath.substring(0, pos);
    //var postPath = strPath.substring(0, strPath.substr(1).indexOf('/') + 1);
    return prePath;
}

//获得完整url
function getUrl(url) {
    var rootUrl = getRootPath();
    var http = "http://";
    if (rootUrl.indexOf("https://") > -1) {
        http = "https://";
    }
    rootUrl = rootUrl.replace(http, "");
    realUrl = rootUrl + "/" + url.replace("~/", "");
    realUrl = realUrl.replace("//", "/");
    return http + realUrl;
}

//根据参数名获得参数
function getQueryString(name) {
    var reg = new RegExp('(^|&)' + name + '=([^&]*)(&|$)', 'i');
    var r = window.location.search.substr(1).match(reg);
    if (r != null) {
        return unescape(r[2]);
    }
    return null;
}

//判断是否为电脑打开
function isPC() {
    var userAgentInfo = navigator.userAgent;
    var Agents = ["Android", "iPhone",
        "SymbianOS", "Windows Phone",
        "iPad", "iPod"];
    var flag = true;
    for (var v = 0; v < Agents.length; v++) {
        if (userAgentInfo.indexOf(Agents[v]) > 0) {
            flag = false;
            break;
        }
    }
    return flag;
}

//判断是否是微信浏览器的函数
function isWeiXin() {
    //window.navigator.userAgent属性包含了浏览器类型、版本、操作系统类型、浏览器引擎类型等信息，这个属性可以用来判断浏览器类型
    var ua = window.navigator.userAgent.toLowerCase();
    //通过正则表达式匹配ua中是否含有MicroMessenger字符串
    if (ua.match(/MicroMessenger/i) == 'micromessenger') {
        return true;
    } else {
        return false;
    }
}

//删除所有HTML标签 
function removeHtmlTab(tab) {
    return tab.replace(/<[^<>]+?>/g, '');
}

function dealErrorInfo(XMLHttpRequest) {
    if (XMLHttpRequest.responseText) {

        $(".btn-loading").each(function () {
            $(this).button("reset");
        });

        swal({
            title: "错误信息",
            text: "请联系管理员:" + XMLHttpRequest.responseText,
            type: "error",
            showCancelButton: false,
            confirmButtonText: "关闭",
            cancelButtonText: "否",
            closeOnConfirm: true,
            closeOnCancel: true,
            customClass: "customError"
        }, function (isConfirm) {

        });

        return;
    }
    showErrorMsg("操作失败", "请联系管理员");
}

function resetLoadButton() {
    setTimeout(resetLoadButtonReal, 100);
}

function resetLoadButtonReal() {
    $(".btn-loading").each(function () {
        $(this).button('reset');
    });
}

//显示错误信息
function showErrorMsg(title, message, delay) {
    resetLoadButton();

    delay = 0;

    if (title == undefined || title == null || title == "") {
        title = "错误";
    }

    $.notify({
        title: title + ": ",
        message: message,
        icon: 'fa fa-close'
    }, {
        type: "danger", //danger warning success info
        delay: delay
    });
}

//显示提示信息
function showInfoMsg(title, message, delay) {
    if (delay == undefined) {
        delay = 5000;
    }
    $.notify({
        title: title + ": ",
        message: message,
        icon: 'fa fa-info'
    }, {
        type: "info",//danger warning success info
        delay: delay
    });
}

//表单验证
function checkValid() {
    $(".needValid").each(function () {

        var objBtn = $(this);

        var validForId = objBtn.attr("validFor");

        if (validForId && validForId != "") {
            //非空验证
            $("#" + validForId).find(".required").each(function () {
                var allValidPass = true;
                $("#" + validForId).find(".required").each(function () {
                    if ($(this).val() == "" || $(this).val() == null) {
                        allValidPass = false;
                    }
                });
                if (allValidPass) {
                    objBtn.removeAttr("disabled");
                }
                else {
                    objBtn.attr("disabled", "disabled");
                }
            });
        }
    });

    $(".needValid").each(function () {

        var objBtn = $(this);

        var validForId = objBtn.attr("validFor");

        if (validForId && validForId != "") {
            //非空验证
            $("#" + validForId).find(".required").each(function () {

                //检验需要验证的控件验证都通过
                $(this).bind('input propertychange change', function () {
                    var allValidPass = true;
                    $("#" + validForId).find(".required").each(function () {
                        if ($(this).val() == "" || $(this).val() == null) {
                            allValidPass = false;
                        }
                    });
                    if (allValidPass) {
                        objBtn.removeAttr("disabled");
                    }
                    else {
                        objBtn.attr("disabled", "disabled");
                    }
                });
            });
        }
    });

}

function validAjaxResult(result) {
    if (!result) {
        return;
    }

    if (!result.result) {
        try {
            result = JSON.parse(result);
        } catch (e) {

        }
    }

    if (result.result == "failure") {
        if (result.msg == "身份过期") {
            var historyUrl = "";

            if (result.urlRedirect) {
                historyUrl = result.urlRedirect;
            }

            window.location.href = "/Account/Login";
        }
    }

}

function validAjaxResultForPC(result) {

    validAjaxResult(result);

    if (result) {
        if (result.result == "failure") {
            showErrorMsg("操作失败", result.msg, 0);
            return;
        }

        if (result.msg && result.msg != "") {
            showInfoMsg("操作成功", result.msg);
            window.setTimeout(function () {
                if (result.urlRedirect && result.urlRedirect != "") {
                    window.location.href = result.urlRedirect;
                }
                else {
                    window.location.reload();
                }
            }, 1000);
            return;
        }

        if (result.urlRedirect && result.urlRedirect != "") {
            window.location.href = result.urlRedirect;
        }
        else {
            window.location.reload();
        }
    }
    else {
        showErrorMsg("操作失败", "服务器未响应");
        return;
    }
}

function showOtherTab(tabName, tabNum, isAutoChangeTab) {

    if (isAutoChangeTab == undefined) {
        isAutoChangeTab = true;
    }

    if (tabNum == undefined) {
        tabNum = 2;
    }

    if (!tabName) {
        tabName = "明细";
    }
    $("#tab" + tabNum + "A").text(tabName);
    var detailHtml = $(".tab" + tabNum + "Content").html();
    $(".tab" + tabNum + "Content").remove();
    $("#tab" + tabNum).html(detailHtml);
    $("#tab" + tabNum + "Title").show();
    var sessionName = "choosetab" + tabNum + "Title_" + window.location.href.split("/")[4];
    if (sessionStorage.getItem(sessionName) == "true" && isAutoChangeTab == true) {
        $("#tab" + tabNum + "A").click();
    }
}

//上传图片到后台
function saveCallBack(base64Url, obj, file) {
    var lastStr = base64Url.substr(base64Url.length - 1, 1);

    if (lastStr == "…") {
        base64Url = base64Url.Substring(0, base64Url.Length - 1);
    }
    var attrId = $(obj).parent().find(".img-base64").val();
    $(obj).parent().find(".btn-img-save").addClass("hidden");
    $(obj).parent().find(".btn-img-resave").addClass("hidden");
    //最终把此base64传给后端
    $.ajax({
        type: "POST",  //提交方式
        url: "/Management/Attachment/SaveImg",//路径
        data: { attrId: attrId, base64Url: base64Url, filename: file.name, filetype: file.type },
        dataType: "json",
        success: function (result) {//返回数据根据结果进行相应的处理
            if (result) {
                if (result.result == "failure") {
                    showErrorMsg("失败", result.msg);
                    return;
                }
                else {
                    if (result.data && result.data.fileId && result.data.fileId != "0") {
                        $(obj).parent().find(".img-base64").val(result.data.fileId);
                        $(obj).parent().find(".img-init").attr("src", base64Url);
                        showInfoMsg("成功", "图片上传成功");
                    }
                    else {
                        showErrorMsg("失败", "图片上传失败");
                    }
                }
            }
            else {
                showErrorMsg("失败", "图片上传失败,未知错误");
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            dealErrorInfo(XMLHttpRequest);
            resetLoadButton();
        }
    });
}

// 不对图片进行压缩，直接转成base64
function directTurnIntoBase64(fileObj, callback) {
    var r = new FileReader();
    // 转成base64
    r.onload = function () {
        //变成字符串
        imgBase64 = r.result;
        console.log(imgBase64);
        callback(imgBase64);
    }
    r.readAsDataURL(fileObj);    //转成Base64格式
}

// 对图片进行压缩
function compressImg(fileObj, width, height, callback) {
    if (typeof (FileReader) === 'undefined') {
        showInfoMsg("信息", "当前浏览器内核不支持base64图标压缩");
        //调用上传方式不压缩  
        directTurnIntoBase64(fileObj, callback);
    } else {
        try {
            var reader = new FileReader();
            reader.onload = function (e) {
                var image = $('<img/>');
                image.load(function () {
                    square = width > height ? width : height,   //定义画布的大小，也就是图片压缩之后的像素
                        canvas = document.createElement('canvas'),
                        context = canvas.getContext('2d'),
                        imageWidth = 0,    //压缩图片的大小
                        imageHeight = 0,
                        offsetX = 0,
                        offsetY = 0,
                        data = '';

                    canvas.width = width;
                    canvas.height = height;
                    context.clearRect(0, 0, width, height);

                    if (this.width > this.height) {
                        imageWidth = Math.round(square * this.width / this.height);
                        imageHeight = square;
                        offsetX = - Math.round((imageWidth - square) / 2);
                    } else {
                        imageHeight = Math.round(square * this.height / this.width);
                        imageWidth = square;
                        offsetY = - Math.round((imageHeight - square) / 2);
                    }
                    context.drawImage(this, offsetX, offsetY, imageWidth, imageHeight);
                    var data = canvas.toDataURL('image/jpeg');
                    //压缩完成执行回调  
                    callback(data);
                });
                image.attr('src', e.target.result);
            };
            reader.readAsDataURL(fileObj);
        } catch (e) {
            showInfoMsg("信息", "压缩失败,图片不压缩直接上传");
            //调用直接上传方式  不压缩 
            directTurnIntoBase64(fileObj, callback);
        }
    }
}

var HtmlUtil = {
    /*1.用浏览器内部转换器实现html转码*/
    htmlEncode: function (html) {
        //1.首先动态创建一个容器标签元素，如DIV
        var temp = document.createElement("div");
        //2.然后将要转换的字符串设置为这个元素的innerText(ie支持)或者textContent(火狐，google支持)
        (temp.textContent != undefined) ? (temp.textContent = html) : (temp.innerText = html);
        //3.最后返回这个元素的innerHTML，即得到经过HTML编码转换的字符串了
        var output = temp.innerHTML;
        temp = null;
        return output;
    },
    /*2.用浏览器内部转换器实现html解码*/
    htmlDecode: function (text) {
        //1.首先动态创建一个容器标签元素，如DIV
        var temp = document.createElement("div");
        //2.然后将要转换的字符串设置为这个元素的innerHTML(ie，火狐，google都支持)
        temp.innerHTML = text;
        //3.最后返回这个元素的innerText(ie支持)或者textContent(火狐，google支持)，即得到经过HTML解码的字符串了。
        var output = temp.innerText || temp.textContent;
        temp = null;
        return output;
    }
};

$(document).ready(function () {
    //按钮操作中效果
    $(".btn-loading").each(function () {
        $(this).attr("data-loading-text", "操作中...");
        $(this).click(function () {
            $(this).button('loading');
        });
    });

    //返回上一页
    $(".btn-back").each(function () {
        $(this).click(function () {
            window.history.go(-1);
        });
    });

    //图片加载失败默认图片
    //elem是js对象 可以用$(elem)转换成jquery对象
    document.addEventListener("error", function (e) {
        var elem = e.target;
        if (elem.tagName.toLowerCase() === 'img') {
            elem.src = "/Content/images/default.png";
        }
    }, true);

    //表单验证
    checkValid();

    //提交表单
    $(".ajaxSave").click(function () {

        var submitUrl = "";
        var formId = "";

        if ($(this).attr("validFor") == "addForm") {
            submitUrl = "Add";
            formId = "addForm";
        }
        else if ($(this).attr("validFor") == "editForm") {
            submitUrl = "Edit";
            formId = "editForm";
        }
        else if ($(this).attr("validFor") == "editFormNoId") {
            submitUrl = "Edit";
            formId = "editFormNoId";
        }
        else if ($(this).attr("validFor") == "deleteForm") {
            submitUrl = "Delete";
            formId = "editForm";

            swal({
                title: "确定?",
                text: "此操作将会删除本条记录!",
                type: "warning",
                showCancelButton: true,
                confirmButtonText: "是",
                cancelButtonText: "否",
                closeOnConfirm: true,
                closeOnCancel: true
            }, function (isConfirm) {
                if (isConfirm) {
                    $.ajax({
                        type: "POST",  //提交方式
                        url: submitUrl,//路径
                        data: $('#' + formId + '').serialize(),
                        dataType: "json",
                        success: function (result) {//返回数据根据结果进行相应的处理

                            validAjaxResultForPC(result);

                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            dealErrorInfo(XMLHttpRequest);
                            resetLoadButton();
                        }
                    });

                } else {
                    resetLoadButton();
                }
            });

            return;
        }

        if (submitUrl == "" || formId == "") {
            showErrorMsg("操作失败", "提交失败");
            resetLoadButton();
            return;
        }

        $.ajax({
            type: "POST",  //提交方式
            url: submitUrl,//路径
            data: $('#' + formId + '').serialize(),
            dataType: "json",
            success: function (result) {//返回数据根据结果进行相应的处理

                validAjaxResultForPC(result);

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                dealErrorInfo(XMLHttpRequest);
                resetLoadButton();
            }
        });
    });

    $(".ajaxSaveForOther").click(function () {

        var submitUrl = "";
        var formId = "";
        var workflowButtonId = 0;
        var workflowButtonText = "";

        var actionName = $(this).attr("actionName");

        if ($(this).hasClass("btn-workflow")) {
            workflowButtonId = $(this).val();
            workflowButtonText = $(this).context.textContent;
        }

        if ($(this).attr("validFor") == "addForm") {
            submitUrl = "AddFor" + actionName;
            formId = "addForm";
        }
        else if ($(this).attr("validFor") == "editForm") {
            submitUrl = "EditFor" + actionName;
            formId = "editForm";
        }
        else if ($(this).attr("validFor") == "deleteForm") {

            submitUrl = "DeleteFor" + actionName;
            formId = "editForm";

            swal({
                title: "确定?",
                text: "此操作将会删除本条记录!",
                type: "warning",
                showCancelButton: true,
                confirmButtonText: "是!",
                cancelButtonText: "否!",
                closeOnConfirm: true,
                closeOnCancel: true
            }, function (isConfirm) {
                if (isConfirm) {
                    $.ajax({
                        type: "POST",  //提交方式
                        url: submitUrl,//路径
                        data: $('#' + formId + '').serialize(),
                        dataType: "json",
                        success: function (result) {//返回数据根据结果进行相应的处理

                            validAjaxResultForPC(result);

                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            dealErrorInfo(XMLHttpRequest);
                            resetLoadButton();
                        }
                    });

                } else {
                    resetLoadButton();
                }
            });

            return;
        }

        if (submitUrl == "" || formId == "") {
            showErrorMsg("操作失败", "提交失败");
            return;
        }

        if (workflowButtonId != 0 && workflowButtonId != undefined && workflowButtonId != null && workflowButtonId != "") {
            swal({
                title: "确定此操作?",
                text: "确定点击" + workflowButtonText + "?",
                type: "warning",
                showCancelButton: true,
                confirmButtonText: "是!",
                cancelButtonText: "否!",
                closeOnConfirm: true,
                closeOnCancel: true
            }, function (isConfirm) {
                if (isConfirm) {
                    $.ajax({
                        type: "POST",  //提交方式
                        url: submitUrl,//路径
                        data: $('#' + formId + '').serialize() + "&workflowButtonId=" + workflowButtonId,
                        dataType: "json",
                        success: function (result) {//返回数据根据结果进行相应的处理

                            validAjaxResultForPC(result);

                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            dealErrorInfo(XMLHttpRequest);
                            resetLoadButton();
                        }
                    });

                } else {
                    resetLoadButton();
                }
            });
        }
        else {
            $.ajax({
                type: "POST",  //提交方式
                url: submitUrl,//路径
                data: $('#' + formId + '').serialize() + "&workflowButtonId=" + workflowButtonId,
                dataType: "json",
                success: function (result) {//返回数据根据结果进行相应的处理

                    validAjaxResultForPC(result);

                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    dealErrorInfo(XMLHttpRequest);
                    resetLoadButton();
                }
            });
        }

    });

    //屏幕大小改变事件
    $(window).resize(function () {
        $(".select2").css("width", "100%");
    });

    function initSelect(obj, entityName) {
        $(obj).select2({
            minimumInputLength: 0,
            ajax: {
                url: '/Management/' + entityName + '/GetListForSelect',
                dataType: "json",
                quietMillis: 250,
                data: function (params) {
                    return {
                        search: params.term, // search term
                        page: params.page || 1
                    };
                },
                processResults: function (data) {
                    return {
                        results: data.items
                    };
                },
                cache: true,
                delay: 250 // wait 250 milliseconds before triggering the request
            },
            escapeMarkup: function (markup) { return markup; },
            minimumInputLength: 0,
            placeholder: '请选择',
            allowClear: false,
            //templateResult: function (row) {
            //    return $('<option id='+row.id+'>'+row.text+'<option>');
            //}
        });
    }

    $(".select2").each(function () {
        var selectEntityName = $(this).attr("selectEntityName");
        if (typeof (selectEntityName) != "undefined" && selectEntityName != "") {
            initSelect(this, $(this).attr("selectEntityName"));
        }
        else {
            $(this).select2();
        }
    });

    //上传图片相关******************************************************
    $(".img-up").each(function () {
        var that = this;
        $(that).change(function () {
            var this2 = this;
            var filePath = $(this2).val();         //获取到input的value，里面是文件的路径
            var fileFormat = filePath.substring(filePath.lastIndexOf(".")).toLowerCase();
            var imgBase64 = '';     //存储图片的imgBase64
            var fileObj = $(this2).context.files[0]; //上传文件的对象,要这样写才行，用jquery写法获取不到对象

            // 检查是否是图片
            if (!fileFormat.match(/.png|.jpg|.jpeg/)) {
                alert('上传错误,文件格式必须为：png/jpg/jpeg');
                return;
            }

            var imgWidth = $(this2).parent().find(".img-init").attr("width");
            var imgHeight = $(this2).parent().find(".img-init").attr("height");

            // 调用函数，对图片进行压缩
            compressImg(fileObj, imgWidth, imgHeight, function (imgBase64) {
                saveCallBack(imgBase64, that, fileObj);
            });
        });

    });

    $(".img-init").each(function () {
        $(this).click(function () {
            $(this).parent().parent().find(".img-up").click();
        });
    });

    //上传图片相关******************************************************

    //取消回车提交表单
    $(document).on("keypress", "form", function (event) {
        if (event.target.localName == "textarea") {
            return true;
        }
        return event.keyCode != 13;
    });

    //密码框点击去除只读属性，失去焦点加上只读属性
    $(".password").focus(function () {
        $(this).removeAttr("readonly");
    });

    $(".password").blur(function () {
        $(this).attr("readonly", "readonly");
    });

    $(".select2WithNoSearchText").each(function () {
        $(this).select2({
            minimumResultsForSearch: -1
        });
    });

    //*************************************************
    //日期控件
    if ($("body").width() >= 800) {
        $(".date").each(function () {
            //执行一个laydate实例
            laydate.render({
                elem: "#" + $(this).attr("id"), //指定元素
                type: 'date',
                btns: ['now', 'confirm'] //内置可识别的值有：clear、now、confirm
            });
        });

        $(".datetime").each(function () {
            //执行一个laydate实例
            laydate.render({
                elem: "#" + $(this).attr("id"), //指定元素
                type: 'datetime',
                btns: ['now', 'confirm']
            });
        });
    }
    else {
        var dateTimeTheme = "material";
        var u = navigator.userAgent;
        var isiOS = !!u.match(/\(i[^;]+;( U;)? CPU.+Mac OS X/); //ios终端
        if (isiOS) {
            dateTimeTheme = "ios";
        }
        mobiscroll.datetime('.date', {
            //maxDate: new Date(curr + 20, 12, 31, 23, 59),
            display: "bottom",
            theme: dateTimeTheme,
            mode: "scroller",
            lang: "zh",
            dateFormat: 'yy-mm-dd', // 日期格式
            dateWheels: 'yymmdd', //面板中日期排列格式
            onSet: function (textVale, inst) { //选中时触发事件

            }
        });
        mobiscroll.datetime('.datetime', {
            //maxDate: new Date(curr + 20, 12, 31, 23, 59),
            display: "bottom",
            theme: dateTimeTheme,
            mode: "scroller",
            lang: "zh",
            dateFormat: 'yy-mm-dd', // 日期格式
            dateWheels: 'yymmdd', //面板中日期排列格式
            timeFormat: "HH:ii",
            timeWheels: "HH:ii",
            onSet: function (textVale, inst) { //选中时触发事件

            }
        });
    }
    //*************************************************

    //**********关闭浏览器事件*****************************
    // 关闭窗口时弹出确认提示
    $(window).bind('beforeunload', function () {
        // 只有在标识变量is_confirm不为false时，才弹出确认提示
        if (window.is_confirm !== false) {

        }
    })
        // mouseleave mouseover事件也可以注册在body、外层容器等元素上
        .bind('mouseover mouseleave', function (event) {
            is_confirm = event.type == 'mouseleave';
        });
    //****************************************

    //******显示隐藏Tab标签*************************************
    $("#tab1Title").click(function () {
        var sessionName2 = "choosetab2Title_" + window.location.href.split("/")[4];
        var sessionName3 = "choosetab3Title_" + window.location.href.split("/")[4];
        var sessionName4 = "choosetab4Title_" + window.location.href.split("/")[4];
        var sessionName5 = "choosetab5Title_" + window.location.href.split("/")[4];
        sessionStorage.setItem(sessionName2, null);
        sessionStorage.setItem(sessionName3, null);
        sessionStorage.setItem(sessionName4, null);
        sessionStorage.setItem(sessionName5, null);
    });

    $("#tab2Title").click(function () {
        var sessionName2 = "choosetab2Title_" + window.location.href.split("/")[4];
        var sessionName3 = "choosetab3Title_" + window.location.href.split("/")[4];
        var sessionName4 = "choosetab4Title_" + window.location.href.split("/")[4];
        var sessionName5 = "choosetab5Title_" + window.location.href.split("/")[4];
        sessionStorage.setItem(sessionName2, "true");
        sessionStorage.setItem(sessionName3, null);
        sessionStorage.setItem(sessionName4, null);
        sessionStorage.setItem(sessionName5, null);
    });

    $("#tab3Title").click(function () {
        var sessionName2 = "choosetab2Title_" + window.location.href.split("/")[4];
        var sessionName3 = "choosetab3Title_" + window.location.href.split("/")[4];
        var sessionName4 = "choosetab4Title_" + window.location.href.split("/")[4];
        var sessionName5 = "choosetab5Title_" + window.location.href.split("/")[4];
        sessionStorage.setItem(sessionName2, null);
        sessionStorage.setItem(sessionName3, "true");
        sessionStorage.setItem(sessionName4, null);
        sessionStorage.setItem(sessionName5, null);
    });

    $("#tab4Title").click(function () {
        var sessionName2 = "choosetab2Title_" + window.location.href.split("/")[4];
        var sessionName3 = "choosetab3Title_" + window.location.href.split("/")[4];
        var sessionName4 = "choosetab4Title_" + window.location.href.split("/")[4];
        var sessionName5 = "choosetab5Title_" + window.location.href.split("/")[4];
        sessionStorage.setItem(sessionName2, null);
        sessionStorage.setItem(sessionName3, null);
        sessionStorage.setItem(sessionName4, "true");
        sessionStorage.setItem(sessionName5, null);
    });

    $("#tab5Title").click(function () {
        var sessionName2 = "choosetab2Title_" + window.location.href.split("/")[4];
        var sessionName3 = "choosetab3Title_" + window.location.href.split("/")[4];
        var sessionName4 = "choosetab4Title_" + window.location.href.split("/")[4];
        var sessionName5 = "choosetab5Title_" + window.location.href.split("/")[4];
        sessionStorage.setItem(sessionName2, null);
        sessionStorage.setItem(sessionName3, null);
        sessionStorage.setItem(sessionName4, null);
        sessionStorage.setItem(sessionName5, "true");
    });

    $("a").click(function () {
        if ($(this).attr("entity") != undefined) {
            var sessionName = "choosetab2Title_" + window.location.href.split("/")[4];
            sessionStorage.setItem(sessionName, null);
            var sessionName3 = "choosetab3Title_" + window.location.href.split("/")[4];
            var sessionName4 = "choosetab4Title_" + window.location.href.split("/")[4];
            var sessionName5 = "choosetab5Title_" + window.location.href.split("/")[4];
            sessionStorage.setItem(sessionName3, null);
            sessionStorage.setItem(sessionName4, null);
            sessionStorage.setItem(sessionName5, null);
        }
    });
    //*******************************************

});
