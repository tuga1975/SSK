var currnoticelist = [];
var newnoticelist = [];
var offsetlist = [];
var noticeCounter = 0;

function updateNoticeBoard(currDate,uri){		
	var newnoticelist = getnotices(currDate,uri);
	
	offsetlist = retuniquerow(newnoticelist, currnoticelist);
	
	offsetlist.sort(function(a, b) {
		return Date.parse(a.Datetime) - Date.parse(b.Datetime);
	});
		
	if(currnoticelist.length == 0){
		clearNoticeBoard();
	}
	
	console.log(offsetlist);
	$.each
	(
		offsetlist, 
		function(i,field)
		{
			var newnotice = $(buildNotice(i,field)).hide();
			$(".noticeboard").prepend(newnotice);
			newnotice.show('slow');
								
			if(field.Type == "error")
			{
				$(".notice"+noticeCounter).children().css({"background-color":"#F77"});
			}
			else
			{
				$(".notice"+noticeCounter).children().css({"background-color":"#7F7"});
			}
			noticeCounter++;
		}
	);	
		
	if(currnoticelist.length == 0 && offsetlist.length == 0){
		$(".noticeboard").html(
			"<div id='noticeNo'>No notifications Today</div>"
		)
	}
	
	currnoticelist = newnoticelist;
};

function retuniquerow(newlist, currlist){
	var offset=[];
	$.each(newlist, function(index,row){
			if(checkIDexist(row.NotificationID, currlist) == false){
				offset.push(row);
			}
		}
	);
	return offset;
}

function checkIDexist(id, list){
	var result = false
	$.each(list, function(index,row){
			if(row.NotificationID == id){
				result = true;
			}
		}
	);
	return result
}

function getnotices(currDate,uri){
	var result;	
	$.ajax(
			{
				async:false,
				type:"POST",				
				url: uri, 
				data:{requestDate: currDate},
				success: function(tmp){result = tmp;}
			}
		);	
	return result;
}

function setUpdatedTime(){
	var now = new Date();
	var timestamp = formattimestamp(now);
	$(".contentheader").html("<div id='updatedtime'>" + timestamp + "</div>");	
}

function buildNotice(i,datarow){
	var result ="";
	result = "<div id='notice' class='notice"+noticeCounter+"'>" 
	//+ "<div id='noticefield'>" + datarow.NotificationID + "</div>"
	+ "<div id='noticefieldlarge'>" + formattimestamp(datarow.Datetime)  + "</div>"
	+ "<div id='noticefield'>" + datarow.Source + "</div>"
	+ "<div id='noticefield'>" + datarow.Type + "</div>"
	+ "<div id='noticecontent'>" + datarow.Content + "</div>"						
	//+ "<div id='noticefield'>" + datarow.notification_code + "</div>"
	+"</div><p><br></p>"	
	return result;
}

function startRefresh(){    
	//clearNoticeBoard();
	setUpdatedTime();
	setTimeout(startRefresh,refreshrate);
    updateNoticeBoard(currDate,getNoticesURI);		
}

$( document ).ready(function(){startRefresh();}) ;

function clearNoticeBoard(){
	$(".noticeboard").html("<div></div>");
}
