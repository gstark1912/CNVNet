$(document).ready(function () {
    //Función del Enter
    $(':text').keypress(function (event) {
        var enterOkClass = $(this).attr('class');
        if (event.which === 13 && enterOkClass !== 'enterSubmit') {
            event.preventDefault();
            return false;
        }
    });

    //Helper Estilos
    $("#body_main_wrapper").css("min-height", $(window).innerHeight() - $("header").height() - $("footer").height() + "px");
    $(window).resize(function () { $("#body_main_wrapper").css("min-height", $(window).innerHeight() - $("header").height() - $("footer").height() + "px"); });
    $("select[multiple='multiple']").css("display", "none");

    //Menus
    var delay;
    $("nav ul li").hover(function () {
        if ($("> ul", this).length > 0) {
            $("> ul", this).slideDown(300);
        }
    }, function () {
        var self = this;
        /*delay = setTimeout(function () {*/
            if ($("> ul", self).length > 0) {
                $(self).children("ul").stop(true, true);
                $("> ul", self).slideUp(200);
            }
        /*}, 150);*/
    });

    //Grilla (ajuste de ancho dinámico)
    $(".ui-jqgrid").each(function () {
        $(this).find(".ui-jqgrid-btable").setGridWidth($(this).parent().parent().width());
    })

    $(window).bind('resize', function () {
        $(".ui-jqgrid").each(function () {
            $(this).find(".ui-jqgrid-btable").setGridWidth($(this).parent().parent().width());
        })
    }).trigger('resize');
});

//Ajuste de Ancho de Grilla
function setRelativeWidth(theGrid) {
    alert("executed");
    $(".ui-jqgrid").each(function () {
        $(this).find(".ui-jqgrid-btable").setGridWidth($(this).parent().parent().width());
    })
    alert("paso");
}

function slideUpNow(currentElement) {
    console.log("funcion");
    
}