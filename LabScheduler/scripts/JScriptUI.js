/*
  Copyright 2017 University of Michigan

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

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

