function fillQuerySettings()
{
	var contains10bd = false;
	$.getJSON("http://10.92.5.27:29000/pathVector/horizonTenors", null, function(data){
        	$.each(data.value, function(i, item){
			if (item == '10bd') contains10bd = true;
        		$('#horizonTenor').append('<option>' + item + '</option>');
                });
		if (contains10bd) $('#horizonTenor').val('10bd');
		$('#bins').val('50');
		$('#ci').val('95%');
		//$('#horizonTenor').selectpicker('refresh');
	});
	
	$.getJSON("http://10.92.5.27:29000/pathVector/dates", null, function(data){
		$.each(data.value, function(i, item){
			var d = new Date(Date.parse(item));
			$('#asOfDates').append('<option>' + (d.getUTCMonth() + 1) + '/' + d.getUTCDate() + '/' + d.getFullYear() + '</option>');
                });
		$('#asOfDates').selectpicker('refresh');
	});

	$.getJSON("http://10.92.5.27:29000/pathVector/entities", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#entities').append('<option>' + item + '</option>');
	        });
		$('#entities').selectpicker('refresh');
		$('#entities').selectpicker('render');
	});
	 
	$.getJSON("http://10.92.5.27:29000/pathVector/counterparties", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#counterparties').append('<option>' + item + '</option>');
		});
		$('#counterparties').selectpicker('refresh');
        });
}

function fillQuerySettings_Exposure()
{
	$.getJSON("http://10.92.5.27:29000/pathVector/dates", null, function(data){
		$.each(data.value, function(i, item){
			var d = new Date(Date.parse(item));
			$('#asOfDates').append('<option>' + (d.getUTCMonth() + 1) + '/' + d.getUTCDate() + '/' + d.getFullYear() + '</option>');
                });
		$('#asOfDates').selectpicker('refresh');
	});

	$.getJSON("http://10.92.5.27:29000/pathVector/entities", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#entities').append('<option>' + item + '</option>');
	        });
		$('#entities').selectpicker('refresh');
		$('#entities').selectpicker('render');
	});
	 
	$.getJSON("http://10.92.5.27:29000/pathVector/counterparties", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#counterparties').append('<option>' + item + '</option>');
		});
		$('#counterparties').selectpicker('refresh');
        });
}

function fillAdvancedQuerySettings()
{
	$.getJSON("http://10.92.5.27:29000/pathVector/csa", null, function(data){
        	$.each(data.value, function(i, item){
        	        $('#csa').append('<option>' + item + '</option>');
        	});
		$('#csa').selectpicker('refresh');
        });
	
	$.getJSON("http://10.92.5.27:29000/pathVector/products", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#products').append('<option>' + item + '</option>');
                });
		$('#products').selectpicker('refresh');
	});

	$.getJSON("http://10.92.5.27:29000/pathVector/portfolios", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#portfolios').append('<option>' + item + '</option>');
		});
		$('#portfolios').selectpicker('refresh');
	});
	
	$.getJSON("http://10.92.5.27:29000/pathVector/trades", null, function(data){
		var insertString;
	        $.each(data.value, function(i, item){
	                insertString += '<option>' + item + '</option>';
		});
		$('#trades').append(insertString);
		$('#trades').selectpicker('refresh');
	});
	
	$.getJSON("http://10.92.5.27:29000/pathVector/category", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#cleared').append('<option>' + item + '</option>');
		});
		$('#cleared').selectpicker('refresh');
	});

	$.getJSON("http://10.92.5.27:29000/pathVector/reportGroup", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#reportGroup').append('<option>' + item + '</option>');
		});
		$('#reportGroup').selectpicker('refresh');
	});

	$.getJSON("http://10.92.5.27:29000/pathVector/gaap", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#stat').append('<option>' + item + '</option>');
		});
		$('#stat').selectpicker('refresh');
	});


	$.getJSON("http://10.92.5.27:29000/pathVector/gaap", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#gaap').append('<option>' + item + '</option>');
		});
		$('#gaap').selectpicker('refresh');
	});
}

function fillAdvancedQuerySettings_Exposure()
{
	$.getJSON("http://10.92.5.27:29000/pathVector/products", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#products').append('<option>' + item + '</option>');
                });
		$('#products').selectpicker('refresh');
	});

	$.getJSON("http://10.92.5.27:29000/pathVector/portfolios", null, function(data){
	        $.each(data.value, function(i, item){
	                $('#portfolios').append('<option>' + item + '</option>');
		});
		$('#portfolios').selectpicker('refresh');
	});
	
	$.getJSON("http://10.92.5.27:29000/pathVector/trades", null, function(data){
		var insertString;
	        $.each(data.value, function(i, item){
	                insertString += '<option>' + item + '</option>';
		});
		$('#trades').append(insertString);
		$('#trades').selectpicker('refresh');
	});
}
