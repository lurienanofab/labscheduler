(function() {
  var template = Handlebars.template, templates = Handlebars.templates = Handlebars.templates || {};
templates['processinfoline'] = template({"1":function(container,depth0,helpers,partials,data) {
    var helper, alias1=depth0 != null ? depth0 : (container.nullContext || {}), alias2=container.hooks.helperMissing, alias3="function", alias4=container.escapeExpression, lookupProperty = container.lookupProperty || function(parent, propertyName) {
        if (Object.prototype.hasOwnProperty.call(parent, propertyName)) {
          return parent[propertyName];
        }
        return undefined
    };

  return "<tr class=\"process-info-line\" data-process-info-line-id=\""
    + alias4(((helper = (helper = lookupProperty(helpers,"processInfoLineId") || (depth0 != null ? lookupProperty(depth0,"processInfoLineId") : depth0)) != null ? helper : alias2),(typeof helper === alias3 ? helper.call(alias1,{"name":"processInfoLineId","hash":{},"data":data,"loc":{"start":{"line":2,"column":57},"end":{"line":2,"column":78}}}) : helper)))
    + "\">\r\n    <td class=\"param\">"
    + alias4(((helper = (helper = lookupProperty(helpers,"param") || (depth0 != null ? lookupProperty(depth0,"param") : depth0)) != null ? helper : alias2),(typeof helper === alias3 ? helper.call(alias1,{"name":"param","hash":{},"data":data,"loc":{"start":{"line":3,"column":22},"end":{"line":3,"column":31}}}) : helper)))
    + "</td>\r\n    <td class=\"min-value\">"
    + alias4(((helper = (helper = lookupProperty(helpers,"minValue") || (depth0 != null ? lookupProperty(depth0,"minValue") : depth0)) != null ? helper : alias2),(typeof helper === alias3 ? helper.call(alias1,{"name":"minValue","hash":{},"data":data,"loc":{"start":{"line":4,"column":26},"end":{"line":4,"column":38}}}) : helper)))
    + "</td>\r\n    <td class=\"max-value\">"
    + alias4(((helper = (helper = lookupProperty(helpers,"maxValue") || (depth0 != null ? lookupProperty(depth0,"maxValue") : depth0)) != null ? helper : alias2),(typeof helper === alias3 ? helper.call(alias1,{"name":"maxValue","hash":{},"data":data,"loc":{"start":{"line":5,"column":26},"end":{"line":5,"column":38}}}) : helper)))
    + "</td>\r\n    <td class=\"edit-delete\">\r\n        <a href=\"#\" class=\"edit-button\"></a>\r\n        <a href=\"#\" class=\"delete-button\"></a>\r\n        <a href=\"#\" class=\"save-button\"></a>\r\n        <a href=\"#\" class=\"cancel-button\"></a>\r\n    </td>\r\n</tr>\r\n";
},"compiler":[8,">= 4.3.0"],"main":function(container,depth0,helpers,partials,data) {
    var stack1, lookupProperty = container.lookupProperty || function(parent, propertyName) {
        if (Object.prototype.hasOwnProperty.call(parent, propertyName)) {
          return parent[propertyName];
        }
        return undefined
    };

  return "﻿"
    + ((stack1 = lookupProperty(helpers,"each").call(depth0 != null ? depth0 : (container.nullContext || {}),(depth0 != null ? lookupProperty(depth0,"lines") : depth0),{"name":"each","hash":{},"fn":container.program(1, data, 0),"inverse":container.noop,"data":data,"loc":{"start":{"line":1,"column":1},"end":{"line":13,"column":9}}})) != null ? stack1 : "");
},"useData":true});
})();