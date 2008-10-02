// ajax services
// from ajax-patterns book/site

// 2007-04-05 (mca) : changed root to 'ajax'
// 2007-04-06 (mca) : added try-catch for http.open
//                  : added status and statusText for return
//                  : changed public methods to http[method]
//                  : added support for sending request header array
// 2007-04-16 (mca) : added support for onTimeout, timeoutMessage, maxTimeout
// 2008-01-21 (mca) : updated _createHeaderMap to return headers as lowercase
// 2008-10-01 (mca) : added support for collecting form values (getFields, getValue)
//                  : added onComplete, showErrors, showStatus
//                  : added support for context = function (_checkType)
//                  : added new functions httpPostForm,httpPutForm,httpDeleteForm,quickPost,quickPut,quickDelete

var ajax = {

  shouldDebug: false,
  shouldEscapeVars: false,
  shouldMakeHeaderMap: true,

  // 2008-10-01 (mca) : toggle alerts for oncomplete
  showErrors: true,
  showStatus: true,
  // 2008-10-01 (mca) : toggle alerts for oncomplete

  // 2007-04-16 (mca) : added to support timeout hook
  maxTimeout: 5000,
  timeoutMessage: 'Unable to complete request - server timed out.',
  onTimeout: null,
  _reqTimer : null,
  // 2007-04-16 (mca) : added to support timeout hook

  calls : new Array(),
  pendingResponseCount : 0,


   /**************************************************************************
      PUBLIC METHODS
   *************************************************************************/

  // http get methods
  httpGet: function(url, urlVars, callbackFunction, expectingXML, callingContext, reqHdrs) {
    return this._callServer(url, urlVars, callbackFunction, expectingXML,
                    callingContext, "GET", null, null, null, reqHdrs);
  },

  httpGetXML: function(url, callbackFunction, reqHdrs) {
    this.httpGet(url, null, callbackFunction, true, null, reqHdrs);
  },

  httpGetPlainText: function(url, callbackFunction, reqHdrs) {
    this.httpGet(url, null, callbackFunction, false, null, reqHdrs);
  },

  // http post methods
  httpPost:
    function(url, optionalURLVars, callbackFunction, expectingXML,
             callingContext, bodyType, body, reqHdrs) {
      this._callServer(url, optionalURLVars, callbackFunction, expectingXML,
                      callingContext, "POST", null, bodyType, body, reqHdrs);
  },

  httpPostForPlainText: function(url, vars, callbackFunction, reqHdrs) {
    this.httpPostVars(url, vars, null, callbackFunction, false,
                    null, "POST", null, null, reqHdrs);
  },

  httpPostForXML: function(url, vars, callbackFunction, reqHdrs) {
    this.httpPostVars(url, vars, null, callbackFunction, true,
                    null, "POST", null, null, reqHdrs);
  },

  httpPostVars:
    function(url, bodyVars, optionalURLVars, callbackFunction, expectingXML,
             callingContext, reqHdrs) {
      this._callServer(url, optionalURLVars, callbackFunction, expectingXML,
                      callingContext, "POST", bodyVars, null, null, reqHdrs);
  },

  httpPostMultiPart:
    function(url, bodyVars, optionalURLVars, callbackFunction, expectingXML,
             callingContext, reqHdrs) {
      this._callServer(url, optionalURLVars, callbackFunction, expectingXML,
                      callingContext, "POST", bodyVars, 'multipart/form-data', null, reqHdrs);
  },

  // other http methods
  httpHead: function(url, urlVars, callbackFunction, expectingXML, callingContext, reqHdrs)
  {
    this._callServer(url, urlVars, callbackFunction, expectingXML,
                    callingContext, "HEAD", null, null, null, reqHdrs);
  },

  httpPut:
    function(url, optionalURLVars, callbackFunction, expectingXML,
             callingContext, bodyType, body, reqHdrs) {
      this._callServer(url, optionalURLVars, callbackFunction, expectingXML,
                      callingContext, "PUT", null, bodyType, body, reqHdrs);
  },

  httpDelete: function(url, urlVars, callbackFunction, expectingXML, callingContext, reqHdrs) {
    this._callServer(url, urlVars, callbackFunction, expectingXML,
                    callingContext, "DELETE", null, null, null, reqHdrs);
  },

  httpOptions:
    function(url, optionalURLVars, callbackFunction, expectingXML,
             callingContext, bodyType, body, reqHdrs) {
      this._callServer(url, optionalURLVars, callbackFunction, expectingXML,
                      callingContext, "OPTIONS", null, bodyType, body, reqHdrs);
  },

  httpTrace:
    function(url, optionalURLVars, callbackFunction, expectingXML,
             callingContext, bodyType, body, reqHdrs) {
      this._debug("trace");
      this._callServer(url, optionalURLVars, callbackFunction, expectingXML,
                      callingContext, "TRACE", null, bodyType, body, reqHdrs);
  },

  // short-cut methods (2008-10-01)
  quickPut:function(formRef,url)
  {
    this.httpPutForm(url, null, null, false, null, null, formRef);
    return false;
  },

  httpPutForm:
    function(url, optionalURLVars, callbackFunction, expectingXML,
             callingContext, bodyType, formRef, reqHdrs) {

      var bodyVars = this.getFields(formRef);
      if(url==null)
      {
        var f = document.getElementById(formRef);
        if(f==null)
        {
          f = document.getElementsByName(formRef)[0];
        }
        url = f.action;
      }
      this._callServer(url, optionalURLVars, callbackFunction, expectingXML,
                      callingContext, "PUT", bodyVars, bodyType, null, reqHdrs);
  },

  quickDelete:function(formRef,url)
  {
    this.httpDeleteForm(url, null, null, false, null, null, formRef);
    return false;
  },

  httpDeleteForm:
    function(url, optionalURLVars, callbackFunction, expectingXML,
             callingContext, bodyType, formRef, reqHdrs) {

      var bodyVars = this.getFields(formRef);
      if(url==null)
      {
        var f = document.getElementById(formRef);
        if(f==null)
        {
          f = document.getElementsByName(formRef)[0];
        }
        url = f.action;
      }
      this._callServer(url, optionalURLVars, callbackFunction, expectingXML,
                    callingContext, "DELETE", null, null, null, reqHdrs);
  },

  quickPost:function(formRef,url)
  {
    this.httpPostForm(url, null, null, false, null, null, formRef);
    return false;
  },

  httpPostForm:
    function(url, optionalURLVars, callbackFunction, expectingXML,
             callingContext, bodyType, formRef, reqHdrs) {

      var bodyVars = this.getFields(formRef);
      if(url==null)
      {
        var f = document.getElementById(formRef);
        if(f==null)
        {
          f = document.getElementsByName(formRef)[0];
        }
        url = f.action;
      }
      this._callServer(url, optionalURLVars, callbackFunction, expectingXML,
                      callingContext, "POST", bodyVars, bodyType, null, reqHdrs);
  },

  onComplete:function(response,headers,context,status,msg)
  {
    switch(status)
    {
      case 0:
        // ie abort code
        break;
      case 200:   // OK
      case 201:   // Created
      case 202:   // Accepted
      case 204:   // NoBody
        if(ajax.showStatus)
        {
          alert(status+'\n'+msg);
        }
        break;
      case 300:
      case 301:
      case 302:
      case 303:
        if(headers["location"])
        {
          location.href=headers["location"];
        }
        break;
      default:    // 400 & 500 errors
        if(headers["location"])
        {
          location.href=headers["location"];
        }
        else
        {
          if(ajax.showErrors)
          {
            alert(status+'\n'+msg);
          }
        }
        break;
    }

    if(ajax._checkType(context)=='function')
    {
      context.apply(context,[response,headers,context,status,msg]);
    }
  },
  // short-cut methods (2008-10-01)

  /**************************************************************************
     PRIVATE METHODS
  *************************************************************************/

   // 2007-04-16 (mca) : added to support timeout hook
  _requestTimeOut: function()
    {
        if(ajax.onTimeout)
           ajax.onTimeout();
        else
            alert('ERROR:\n'+ajax.timeoutMessage);
        //xReq.abort(); // commented out to prevent IE wierdness
    },
   // 2007-04-16 (mca) : added to support timeout hook

  _callServer: function(url, urlVars, callbackFunction, expectingXML,
                       callingContext, requestMethod, bodyVars,
                       explicitBodyType, explicitBody, reqHdrs) {

    if (urlVars==null) {
      urlVars = new Array();
    }

    if(callbackFunction==null)
    {
      callbackFunction = this.onComplete;
    }

    this._debug("_callServer() called. About to request URL\n"
                + "call key: [" + this.calls.length + "]\n"
                + "url: [" + url + "]\n"
                + "callback function: [" + callbackFunction + "]\n"
                + "treat response as xml?: [" + expectingXML + "]\n"
                + "Request method?: [" + requestMethod + "]\n"
                + "calling context: [" + callingContext + "]\n"
                + "explicit body type: [" + explicitBodyType + "]\n"
                + "explicit body: [" + explicitBody + "]\n"
                + "urlVars: [" + this._describe(urlVars) + "]\n"
                + "bodyVars: [" + this._describe(bodyVars) + "]\n"
                + "reqHdrs: [" + this._describe(reqHdrs) + "]"
              );


    var xReq = this._createXMLHttpRequest();
    xReq.onreadystatechange = function() {
      ajax._onResponseStateChange(call);
    }

    var call = {xReq: xReq,
                callbackFunction: callbackFunction,
                expectingXML: expectingXML,
                callingContext: callingContext,
                url: url};

    if (urlVars!=null) {
      var urlVarsString = this._createHTTPVarSpec(urlVars);
      if (urlVarsString.length > 0) { // TODO check if appending with & instead
        url += "?" + urlVarsString;
      }
    }

    // 2007-04-06 (mca) : added trap for fatal open errors
    try
    {
        xReq.open(requestMethod, url, true);
    }
    catch(ex)
    {
        alert(
            'ERROR:\nXMLHttpRequest.open failed for\n'+
            url+' ['+requestMethod+']'+'\n'+
            ex.message
            );
        return;
    }
    // 2007-04-06 (mca) : added trap for fatal open errors

    // 2007-04-16 (mca) : added to support timeout hook
    //ajax._reqTimer = setTimeout(ajax._requestTimeOut,ajax.maxTimeout);
    // 2007-04-16 (mca) : added to support timeout hook

    if(reqHdrs!=null)
        this._appendHeaders(xReq,reqHdrs);

    if (   requestMethod=="GET"
        || requestMethod=="HEAD"
        || requestMethod=="DELETE") {
      this._debug("Body-less request to URL " + url);
      xReq.send(null);
      return call;  // return call object
    }

    if (   requestMethod=="POST"
        || requestMethod=="PUT"
        || requestMethod=="OPTIONS"
        || requestMethod=="TRACE") {
      bodyType = null;
      body = null;
      if (explicitBodyType==null) { // It's a form
        bodyType = 'application/x-www-form-urlencoded; charset=UTF-8';
        body = this._createHTTPVarSpec(bodyVars);
      } else {
        bodyType = explicitBodyType;
        body = explicitBody;
      }
      this._debug("Content-Type: [" + bodyType + "]\nBody: [" + body + "].");
      xReq.setRequestHeader('Content-Type',  bodyType);
      xReq.send(body);
      return call;  // return call object
    }

    this._debug("ERROR: Unknown Request Method: " + requestMethod);

  },

  // The callback of xmlHttpRequest is a dynamically-generated function which
  // immediately calls this function.
  _onResponseStateChange: function(call) {

    xReq = call.xReq;

    if (xReq.readyState < 4) { //Still waiting
      return;
    }

    // 2007-04-16 (mca) : added to support timeout hook
    clearTimeout(this._reqTimer);
    // 2007-04-16 (mca) : added to support timeout hook

    if (xReq.readyState == 4) { //Transmit to actual callback
      this._debug("Call " + this._describe(call)
                + " with context [" + call.callingContext+"]"
                + " to " + call.url + " has returned.");
      callbackFunction = call.callbackFunction;
      if (!callbackFunction) { // Maybe still loading, e.g. in another JS file
        setTimeout(function() {
          _onResponseStateChange(call);
        }, 100);
      }
      var content = call.expectingXML ? xReq.responseXML : xReq.responseText;
      responseHeaders = xReq.getAllResponseHeaders();
      headersForCaller = this.shouldMakeHeaderMap ?
        this._createHeaderMap(responseHeaders) : responseHeaders;
      // 2007-04-06 (mca) : added status and statusText as optional returns
      callbackFunction(content, headersForCaller, call.callingContext, xReq.status, xReq.statusText);
    }

    call = null; // Technically the responsibility of GC
    this.pendingResponseCount--;

  },

  // short-cut methods (2008-10-01)
  getFields:function(form)
  {
    var formElement,coll,i,vars,item,val;
    vars = {};

    try
    {
      formElement = document.getElementById(form);
      if(!formElement)
      {
        formElement = document.getElementsByName(form)[0];
      }
      coll = formElement.getElementsByTagName('*');
      for(i=0;i<coll.length;i++)
      {
        item = coll[i];
        if(item.getAttribute('name'))
        {
          val = this.getValue(item);
          if(vars[item.getAttribute('name')])
          {
            if(val!='')
            {
              vars[item.getAttribute('name')] = vars[item.getAttribute('name')]+';'+val;
            }
          }
          else
          {
            vars[item.getAttribute('name')] = val;
          }
        }
      }
    }
    catch(ex)
    {
      alert('getFields Error:\n'+ex.message);
    }

    return vars;
  },

  getValue:function(inputElement)
  {
    var val,i;

    try
    {
      if((inputElement.type=='checkbox' || inputElement.type=='radio') && inputElement.checked==false)
      {
        val='';
      }
      else
      {
        if(inputElement.tagName.toLowerCase()=='select')
        {
          val = '';
          for(i=0;i<inputElement.options.length;i++)
          {
            if(inputElement.options[i].selected==true)
            {
              if(val!='')
              {
                val += ';' + escape(inputElement.options[i].value);
              }
              else
              {
                val = escape(inputElement.options[i].value);
              }
            }
          }
        }
        else
        {
          val = escape(inputElement.value)
        }
      }
    }
    catch(ex)
    {
      alert('getValue Error:\n'+ex.message);
    }

    return val;
  },

  _checkType:function(value) {
      var s = typeof value;
      if (s === 'object') {
          if (value) {
              if (typeof value.length === 'number' &&
                      !(value.propertyIsEnumerable('length')) &&
                      typeof value.splice === 'function') {
                  s = 'array';
              }
          } else {
              s = 'null';
          }
      }
      return s;
  },
  // short-cut methods (2008-10-01)

  // Browser-agnostic factory function
  _createXMLHttpRequest: function() {
    if (window.XMLHttpRequest) {
      return new XMLHttpRequest();
    } else if (window.ActiveXObject) {
      return new ActiveXObject('Microsoft.XMLHTTP')
    } else {
      _error("Could not create XMLHttpRequest on this browser");
      return null;
    }
  },

  _createHTTPVarSpec: function(vars) {
      var varsString = "";
      for( key in vars ) {
        var value = vars[key];
        if (this.shouldEscapeVars) {
          escapePlusRE =  new RegExp("\\\+");
          value = value.replace(escapePlusRE, "%2B");
        }
        varsString += '&' + key + '=' + value;
      }
      if (varsString.length > 0) {
        varsString = varsString.substring(1); // chomp initial '&'
      }
      this._debug("Built var String: " + varsString)
      return varsString;
   },

  /* Creates associative array from header type to header */
  _createHeaderMap: function(headersText) {
    extractedHeaders = headersText.split("\n");
    delete extractedHeaders[extractedHeaders.length]; // Del blank line at end
    headerMap = new Array();
    for (i=0; i<extractedHeaders.length-2; i++) {
      head = extractedHeaders[i];
      fieldNameEnding = head.indexOf(":");
      field = head.substring(0, fieldNameEnding);
      field = field.toLowerCase(); // 2008-01-21 (mca)
      value = head.substring(fieldNameEnding + 2, head.length);
      value = value.replace(/\s$/, "");
      headerMap[field] = value;
    }
    return headerMap;
  },

  _appendHeaders:function(xReq,rh)
  {
    for(key in rh)
        xReq.setRequestHeader(key,rh[key]);
    //return xReq;
  },

  _debug: function(message) {
      if (this.shouldDebug) {
        alert("AjaxJS Message:\n\n" + message);
      }
  },

  _error: function(message) {
      if (this.shouldDebug) {
        alert("AjaxJS ERROR:\n\n" + message);
      }
  },

  _describe: function(obj) {
    if (obj==null) { return null; }
    switch(typeof(obj)) {
      case 'object': {
        var message = "";
        for (key in obj) {
          message += ", [" + key + "]: [" + obj[key] + "]";
        }
        if (message.length > 0) {
          message = message.substring(2); // chomp initial ', '
        }
        return message;
      }
      default: return "" + obj;
    }
  }
};
