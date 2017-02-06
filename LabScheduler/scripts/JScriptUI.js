// JScript File

// Numeric Integer Input
function IntegerInput(eventObj, positive)
{
	// Check For Browser Type
    var keyCode
    if (document.all)
        keyCode = eventObj.keyCode;
    else
        keyCode = eventObj.which;
    
    // Allow numbers only (minus sign if not positive)
    if ((keyCode >= 48 && keyCode <= 57) || (positive == false && keyCode == 45))
        return true;
        
    return false;
}

// Numeric Decimal Input
function DecimalInput(eventObj, positive, decimalpoints)
{
    // Check for Browser
    var keyCode
    if (document.all)
        keyCode = eventObj.keyCode;
    else
        keyCode = eventObj.which;
        
    // Allow numbers and decimal
    if ((keyCode >= 48 && keyCode <= 57) || (positive == false && keyCode == 45))
        return true;
        
    return false;
}

