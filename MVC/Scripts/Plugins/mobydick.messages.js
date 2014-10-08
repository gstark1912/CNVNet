//Mensajes Modal
function ExecuteMessagesSuccess(data, successFunction) {
    CheckForMessages(data, successFunction);
}

function ExecuteMessagesAfterSubmit(data, successFunction) {
    CheckForMessages(JSON.parse(data.responseText), successFunction);
    $(".ui-icon-closethick").trigger('click');
}

function CheckForMessages(data, successFunction) {
    if (eval(data.Success) != false) {
        if (eval(successFunction) != undefined)
            successFunction(data);
    }
    else {
        if (data.Partial != undefined)
            MostrarExceptionTwo(data.Partial);
        if (data.UserMessage != undefined)
            MostrarMensaje(data.UserMessage);
    }
}

function MostrarExceptionTwo(data) {
    $.MDmessage({
        messageType: "error",
        message: data,
        title: 'Error',
        width: 30
    });
};

function MostrarMensajeLoading() {
    $.MDmessage({
        messageType: "popup",
        message: $("#ProgressDialog").html(),
        name: 'ajaxLoading',
        btnok: false,
        width: 15
    });
    return true;
};

function MostrarMensaje(mensaje) {
    $.MDmessage({
        messageType: "info",
        message: '<p class="mdm_text">' + mensaje + '</p>',
        title: 'Información',
    });
};

function ShowAlert(mensaje) {
    $.MDmessage({
        messageType: "info",
        message: '<p class="mdm_text">' + mensaje + '</p>',
        title: 'Información'
    });
};

//Plugin MD Messages (v1.0.0)
(function ($) {
    $.MDmessage = function (settings) {
        // settings
        var config = {
            'messageType': "error", //alert, info o popup
            'title': null, //titulo del mensaje (si es null, queda sin barra de título)
            'buttons': [], //array de botones con funciones
            'btnok': true, //botón que cierra el mensaje
            'btnokText': "Aceptar", //texto del botón que cierra el mensaje
            'message': "Éste es un mensaje por default, reemplazar en la propiedad 'message'", //texto del mensaje
            'width': 40, //Ancho fijo de 0 a 100 (porcentaje relativo al 100% de la pantalla)
            'maxwidth': 100, //Ancho máximo de 0 a 100 (porcentaje relativo al 100% de la pantalla)
            'content': null, //Permite cargar Partials (como reemplazo del message).
            'modal': true, //Con o sin blackout
            'ic': null, //Contador de ID (autoincremental)
            'name': null, //ID custom (en realidad se agrega como una clase) para identificar desde código al elemento creado.
        };
        if (settings) { $.extend(config, settings); }

        //incrementa el valor para asignarle un ID diferente a cada mensaje
        config.ic = $(".md_message").length + 1;
        if (config.messageType != "popup") {
            //<p class="mdm_text">'+config.message+'</p>
            $('body').append('<div id="mdm' + config.ic + '" class="md_message ' + config.messageType + '_message ' + config.name + '" style="display:none; z-index: ' + (1000 + config.ic) + '; max-width:' + config.maxwidth + '%; width:' + config.width + '%;"><p class="mdm_title ' + config.messageType + '_message_title">' + config.title + '</p>' + config.message + '<div class="btns_container"><a id="okbtn_default" href="javascript:closeMessage(&quot;#mdm' + config.ic + '&quot;);" class="button_ui btn_message">' + config.btnokText + '</a></div></div>');
        } else {
            $('body').append('<div id="mdm' + config.ic + '" class="md_message ' + config.messageType + '_message ' + config.name + '" style="display:none; z-index: ' + (1000 + config.ic) + '; max-width:' + config.maxwidth + '%; width:' + config.width + '%;"><p class="mdm_title ' + config.messageType + '_message_title">' + config.title + '</p><div class="mdm_text"><p style="text-align: center; padding: 20px 60px 0 60px;"><img src="' + AppURL("Content/Images/ajax-loader.gif") + '" /></p></div><div class="btns_container"><a id="okbtn_default" href="javascript:closeMessage(&quot;#mdm' + config.ic + '&quot;);" class="button_ui btn_message">' + config.btnokText + '</a></div></div>');
            $('.mdm_text').load(config.content, function (response, status, xhr) {
                if (status == "error") {
                    $('.mdm_text').empty();
                    $('.mdm_text').append("<p style='padding-left: 18px; margin: 20px 10px 10px 10px; background: url(" + AppURL("Content/Images/error.png") + ") no-repeat left center;'>" + xhr.status + " " + xhr.statusText + "</p>");
                    $("#okbtn_default").css("display", "block");
                    centerPopUp("#mdm" + config.ic);
                } else {
                    centerPopUp("#mdm" + config.ic);
                }
            });
            if (config.title == null) {
                $("#mdm" + config.ic + " .mdm_title").remove();
            };
        }

        if (config.btnok == false) {
            $("#okbtn_default").remove();
        };

        $.each(config.buttons, function (name, props) {
            var click, buttonOptions;
            props = $.isFunction(props) ?
                { click: props, text: name } :
            props;
            props = $.extend({ class: "button_ui btn_message", href: "javascript:void(0)" }, props);
            click = props.click;
            props.click = function () {
                click.apply($('#mdm' + config.ic).find('.btn_message'), arguments);
            };
            $("<a></a>", props)
                .appendTo($("#mdm" + config.ic).find(".btns_container"));
        });

        if (config.modal == true) {
            $('body').append('<div class="blackout ' + config.name + 'bo" id="mdm' + config.ic + 'bo" style="display:none; z-index: ' + (999 + config.ic) + ';"></div>');
        }

        $(".blackout").slideDown();
        $("#mdm" + config.ic).fadeIn();

        centerPopUp("#mdm" + config.ic);
    };
})(jQuery);

$(window).resize(function () { centerPopUp(".md_message"); })

function centerPopUp(popup) {
    $(popup).css({
        "top": $(window).innerHeight() / 2 - $(popup).height() / 2 + "px",
        "left": $(window).innerWidth() / 2 - $(popup).width() / 2 + "px",
    });
};

function closeMessage(toClose) {
    if (!(toClose.substr(0, 1) == "#")) {
        $("." + toClose).fadeOut(400);
        $("." + toClose + "bo").slideUp(600, function () {
            $("." + toClose + "bo").remove();
            $("." + toClose).remove();
        });
    } else {
        $(toClose + "").fadeOut(400);
        $(toClose + "bo").slideUp(600, function () {
            $(toClose + "bo").remove();
            $(toClose + "").remove();
        });
    }
}

$(document).keyup(function (e) {
    if (e.keyCode == 27 && $(".md_message").length > 0) {
        var index_highest = new Array();
        var index_current = new Array();
        index_highest[0] = 0;
        $(".md_message").each(function () {
            index_current[0] = parseInt($(this).css("zIndex"), 10);
            index_current[1] = $(this).attr("id");
            if (index_current[0] > index_highest[0]) {
                index_highest[0] = index_current[0];
                index_highest[1] = index_current[1];
            }
        });
        closeMessage("#" + index_highest[1]);
    }
});

// Llamada ajax POST personalizada 
function ajaxCall(options) {
    var defaults = { url: '', data: {}, success: function () { }, showStatus: true, async: true, dataType: 'Json', contentType: 'application/x-www-form-urlencoded; charset=UTF-8' };
    var settings = $.extend({}, defaults, options);

    if (settings.showStatus) {
        _ajaxLoading(settings);
    }
    else {
        _ajax(settings);
    }
}

(function ($) { $.fn.dataBind = function () { } })(jQuery);

function _ajaxLoading(_settings) {
    MostrarMensajeLoading();    
    _ajax(_settings);    
}


function _ajaxStopLoading() {
    closeMessage('ajaxLoading');
}

function _ajax(settings) {
    $.ajax({
        url: settings.url,
        contentType: settings.contentType,
        data: settings.data,
        type: "POST",
        async: settings.async,
        dataType: "Json",
        success: function (data, status, jqXHR) {
            _ajaxStopLoading();
            ExecuteMessagesSuccess(data, settings.success);
        }
    });
}