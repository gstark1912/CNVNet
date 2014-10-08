using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace MvcHtmlHelpers
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString MDImageLoader(this HtmlHelper htmlHelper, string expression, bool ReadOnly)
        {
            return new MvcHtmlString(

                 "<p class='imgWrapper'><img class='imgBoxed allow_zoom' id=" + "imgCar" + " alt='' src='"
                 +
                 expression
                 +
                 "' /></p>"
                 );
        }

        public static MvcHtmlString MDImageLoader(this HtmlHelper htmlHelper, string expression)
        {
            return new MvcHtmlString(
                "<input type='file' id=" + "theFile" + " name=" + "theFile" + " onchange='readURL(this);' />" +
                "<input type='button' value='Quitar Foto' onclick='RemovePicture()' />" +
                "<p class='imgWrapper'><img class='imgBoxed allow_zoom' id=" + "imgCar" + " alt='' src='"
                +
                expression
                +
                "' /></p>"
                +
                "<script type='text/javascript'>"
                +
                "function readURL(input) { if (input.files && input.files[0]) { var reader = new FileReader(); reader.onload = function (e) { $('#" + "imgCar" + "').attr('src', e.target.result); document.getElementById('ExistImage').value = e.target.result; }; reader.readAsDataURL(input.files[0]); }} function RemovePicture() { $('#" + "imgCar" + "').attr('src', '../Content/css/Images/default_car.jpg'); document.getElementById('image').value = ''; }"
                +
                "</script>"
                );
        }

        public static MvcHtmlString MDGoBackButton(this HtmlHelper htmlHelper)
        {
            return MDGoBackButton(htmlHelper, "javascript:history.go(-1)");
        }

        public static MvcHtmlString MDGoBackButton(this HtmlHelper htmlHelper, string url)
        {
            string html = "<a class='btn_secundario btn-right' href='" + url + "'>Volver</a>";

            return new MvcHtmlString(html);
        }

        public static MvcHtmlString MDCheckBoxFor(this HtmlHelper htmlHelper, string label, string name)
        {
            return new MvcHtmlString(
                "<div class='checkbox'><input type='checkbox' name=" + name + " id=" + name + " /><label for=" + name + "></label><span>" + label + "</span></div>"
            );
        }

        public static MvcHtmlString MD2CheckBox<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, bool>> property)
        {
            string name = ExpressionHelper.GetExpressionText(property);
            string id = name.Replace('.', '_');
            string resultado = "<div class='checkbox'>";
            resultado += "<input type='hidden' name='" + name + "'/>";
            resultado += "<input type='checkbox' id='" + id + "' name='" + name + "'/>";
            resultado += "<label for=" + id + "></label><span></span></div>";
            return new MvcHtmlString(resultado);
        }

        public static MvcHtmlString RequiredMarkDescription(this HtmlHelper html)
        {
            return MvcHtmlString.Create("<span style=\"color:red;\">*</span> obligatorios.");
        }

        public static MvcHtmlString LabelForRequired<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText = "")
        {
            return LabelHelper(html, ModelMetadata.FromLambdaExpression(expression, html.ViewData), ExpressionHelper.GetExpressionText(expression), labelText);
        }

        private static MvcHtmlString LabelHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string labelText)
        {
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            }

            if (string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            bool isRequired = false;

            if (metadata.ContainerType != null)
            {
                isRequired = metadata.ContainerType.GetProperty(metadata.PropertyName)
                                .GetCustomAttributes(typeof(RequiredAttribute), false)
                                .Length == 1;
            }

            TagBuilder tag = new TagBuilder("label");

            tag.Attributes.Add(
                "for",
                TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName))
            );

            if (isRequired)
                tag.Attributes.Add("class", "label-required");

            tag.SetInnerText(labelText);

            var output = tag.ToString(TagRenderMode.Normal);

            if (isRequired)
            {
                var asteriskTag = new TagBuilder("span");
                asteriskTag.Attributes.Add("class", "span required");
                //asteriskTag.SetInnerText(" *");
                output += asteriskTag.ToString(TagRenderMode.Normal);
            }

            return MvcHtmlString.Create(output);
        }


    }

    public class HFXRemote : RemoteAttribute
    {
        public HFXRemote(string action, string controller, string area)
            : base(action, controller, area)
        {
            if (!string.IsNullOrWhiteSpace(area) || area == "")
                this.RouteData["area"] = area;
        }
    }

    public class HFXInteger : RegularExpressionAttribute
    {
        static HFXInteger()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(HFXInteger), typeof(RegularExpressionAttributeAdapter));
        }

        public HFXInteger()
            : base("[0-9]{1,}")
        {
            if (this.ErrorMessage == null)
                this.ErrorMessage = "El campo {0} debe ser un número entero";
        }
    }

    public class HFXDate : RegularExpressionAttribute
    {
        static HFXDate()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(HFXDate), typeof(RegularExpressionAttributeAdapter));
        }

        public HFXDate()
            : base(@"^([1-9]|0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.](19|20)\d\d$")
        {
            if (this.ErrorMessage == null)
                this.ErrorMessage = "El campo {0} debe ser una fecha válida";
        }
    }
}