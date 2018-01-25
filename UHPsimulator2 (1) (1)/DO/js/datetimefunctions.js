function formattimestamp(time)
{
	var dtTime =  new Date(time);
	var dd = retPaddedZero(dtTime.getDate());
	var mm = retPaddedZero(dtTime.getMonth() + 1);
	var yyyy = retPaddedZero(dtTime.getFullYear());
	var HH = retPaddedZero(dtTime.getHours());
	var min = retPaddedZero(dtTime.getMinutes());
	var ss = retPaddedZero(dtTime.getSeconds());
		
	return dd + '/' + mm + '/' + yyyy + " " + HH + ":" + min + ":" + ss;
}

function retPaddedZero(num)
{
	if(num < 10)
	{
		num = "0" + num;
	}
	 return num;
}
