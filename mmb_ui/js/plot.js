function plotHistogram(_asOfDate, _entities, _counterparties, _csa, _products, _portfolios, _trades, _cleared, _reportGroup, _strategies, _gaap, _stat, _horizon, bins, confidence)
{
	var binData = [];
	var binMin = [];
	var varPoint, varBin, varLabel, ctePoint, cteBin, cteLabel, varPstring, ctePstring;
	var minPoint, maxPoint, binWidth, formatString, xAxisLabel;
	var data = new Object();
	var chart = $('container').highcharts();
	data.asOfDate = _asOfDate;
        data.entity = _entities;
        data.counterparty = _counterparties;
        data.csa = _csa;
        data.product = _products;
        data.portfolio =  _portfolios;
        data.trade = _trades;
        data.cleared = _cleared; 
        data.reportGroup = _reportGroup;
        data.strategy = "";
        data.gaap = _gaap;
        data.stat = "";
       	data.horizon = _horizon;
	var posting = $.post( "http://10.92.5.27:29000/pathVector/post", data, function(result) {
		var d = $.parseJSON(result);
		bins = $('#bins').val();
		if ($('#ci').val().indexOf('%') != -1) confidence = parseFloat($('#ci').val().replace('%')) / 100;
		else confidence = $('#ci').val();
		minPoint = parseFloat(d.pathValues[0]);
		maxPoint = parseFloat(d.pathValues[d.paths - 1]);
		binWidth = (maxPoint - minPoint) / bins;
		for (var i = 0; i < bins; i++)
			if (i == 0)	{ binMin[i] = minPoint; binData[i] = 0; }
			else { binMin[i] = binMin[i - 1] + binWidth; binData[i] = 0; }
		for (var j = 0; j < parseInt(d.paths); j++)
			for (var i = 0; i < bins; i++)
				if (j == (parseInt(d.paths) - 1)) { binData[bins - 1] += 1; break; }
				else if (parseFloat(d.pathValues[j]) <= binMin[i]) { binData[i] += 1; break; }
		
		if (confidence < 0.5)
		{
			varLabel = Math.round(confidence * 1000) / 10 + '% Exposure';
			cteLabel = Math.round(confidence * 1000) / 10 + '% CTE';
			var tempTotal = 0;
			for (var i = Math.round((1 - confidence) * parseInt(d.paths)); i < parseInt(d.paths); i++)
				tempTotal += parseFloat(d.pathValues[i]);
			ctePoint = tempTotal / (parseInt(d.paths) - Math.round((1 - confidence) * parseInt(d.paths)) - 1);
		}
		else if (confidence > 0.5) 
		{
			varLabel = Math.round(confidence * 1000) / 10 + '% VaR';
			cteLabel = Math.round(confidence * 1000) / 10 + '% CTE';
			var tempTotal = 0;
			for (var i = ((1 - confidence) * parseInt(d.paths)).toFixed(0); i >= 0; i--)
				tempTotal += parseFloat(d.pathValues[i]);

			ctePoint = tempTotal / (Math.round((1 - confidence) * parseInt(d.paths)) + 1);
		}
		else 
		{
			varLabel = 'Median';
			cteLabel = '50% CTE';
			var tempTotal = 0;
			for (var i = parseInt(d.paths) - 1; i >= 0; i--)
				tempTotal += parseFloat(d.pathValues[i]);
			ctePoint = tempTotal / (Math.round((1 - confidence) * parseInt(d.paths)) + 1);
		}
		
		if ((1 - confidence) * parseInt(d.paths) % 1 == 0)
			varPoint = parseFloat(d.pathValues[(1 - confidence) * parseInt(d.paths)]);
		else
			varPoint = (parseFloat(d.pathValues[((1 - confidence) * parseInt(d.paths)).toFixed(0)]) + 
				parseFloat(d.pathValues[Math.round((1 - confidence) * parseInt(d.paths))])) / 2;
		for (var i = 0; i < bins; i++)
			if (varPoint <= binMin[i]) { varBin = i; break; }
			else varBin = i;
		for (var i = 0; i < bins; i++)
			if (ctePoint <= binMin[i]) { cteBin = i; break; }
			else cteBin = i;
		
		if (maxPoint > 10000000) 
		{ 
			formatString = '{value:,.1f}'; 
			xAxisLabel = 'Scenario Value (mm)';
			for(var i = 0; i < binMin.length; i++)
				binMin[i] = binMin[i]/1000000;
		}
		else { formatString = '{value:,.0f}'; xAxisLabel = 'Scenario Value'; }
   		var chart2 = new Highcharts.Chart({
            chart: { renderTo: 'container' },
            title: { text: null },
			legend: {
                layout: 'vertical',
                align: 'left',
                verticalAlign: 'top',
                x: 100,
                y: 70,
                floating: true,
                backgroundColor: '#FFFFFF',
                borderWidth: 1
            },
            xAxis: {
                categories: binMin,
				title: { enabled: true, text: xAxisLabel },
                labels: 
				{ 
					overflow: 'justify',
					align: 'left', 	
					format: formatString, 
					maxStaggerLines: 1, 
					x: 15, 
					y: 15,
					rotation: 45, 
					step: 2
				}
            },
			yAxis: { title: { enabled: false } },
	credits: { enabled: false },
	plotOptions:{
		series:{ pointPadding:0.07, groupPadding:0, borderWidth:0, pointPlacement:'between' },
	},
        series: [
			{ type: 'column', data: binData, showInLegend: false, name: 'Observations',
				tooltip: { headerFormat: '' } 
			},
			{
                type: 'scatter',
                name: varLabel,
                data: [[varBin, 0]],
                marker: {
                	lineWidth: 2,
                	lineColor: Highcharts.getOptions().colors[3],
                	fillColor: 'white',
					radius: 5
                },
				tooltip: {
					headerFormat: '<b>{series.name}</b><br>',
                    pointFormat: '<b>' + varPoint.toFixed(0).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") + '</b>'
				}
            },
			{
                type: 'scatter',
                name: cteLabel,
                data: [[cteBin, 0]],
                marker: {
                	lineWidth: 2,
                	lineColor: Highcharts.getOptions().colors[3],
                	fillColor: 'white',
					radius: 4
                },
				tooltip: {
					headerFormat: '<b>{series.name}</b><br>',
                    pointFormat: '<b>' + ctePoint.toFixed(0).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") + '</b>'
				}
            }]
        });
	}).fail(function(a,b,c) {
    	console.log(a);
    	console.log(b);
    	console.log(c);}, "json" );
}

function plotExposureCurve(_asOfDate, _entities, _counterparties, _products, _portfolios, _trades, _confidence, _expectation)
{
	var binData = [];
	var binMin = [];
	var varPoint, varBin, varLabel, ctePoint, cteBin, cteLabel, varPstring, ctePstring;
	var minPoint, maxPoint, binWidth, formatString, xAxisLabel;
	var data = new Object();
	var chart = $('container').highcharts();
	data.asOfDate = _asOfDate;
    data.entity = _entities;
    data.counterparty = _counterparties;
    data.csa = "";
    data.product = _products;
    data.portfolio =  _portfolios;
    data.trade = _trades;
    data.cleared = ""; 
    data.reportGroup = "";
    data.strategy = "";
    data.gaap = "";
    data.stat = "";
    data.horizon = "";
	data.confidence = _confidence;
	data.expectation = _expectation;
	var posting = $.post( "http://10.92.5.27:29000/exposureCurve/post", data, function(result)
	{
		var r = $.parseJSON(result);
		var exposureData = [];
		var dateData = [];
		var expectationData = [];
		var allData = [];
		for (var i = 0; i < r.key.length; i++)
		{
			if (r.value[i].split("|").length == 1)
			{
				dateData.push(new Date(String(r.key[i]).replace("Date", "").replace("/", "").replace("-0000", "")));
				var d = (dateData[i].getUTCMonth() + 1) + '/' + dateData[i].getUTCDate() + '/' + dateData[i].getFullYear();
				exposureData.push({x: dateData[i], y: r.value[i], name: d });
			}
			else
			{
				dateData.push(new Date(parseFloat(String(r.key[i]).replace("Date", "").replace("/", "").replace("-0000", "").replace("(", "").replace(")", "").replace("/", ""))));
				var d = (dateData[i].getUTCMonth() + 1) + '/' + dateData[i].getUTCDate() + '/' + dateData[i].getFullYear();
				expectationData.push( { x: dateData[i], y: parseFloat(r.value[i].split('|')[0]), name: d } );
				exposureData.push( { x: dateData[i], y: parseFloat(r.value[i].split('|')[1]), name: d } );
			}
		}
		
		if (_expectation == "true")
		{
			allData.push( { data: exposureData, name: 'Exposure' } );
			allData.push( { data: expectationData, name: 'Expectation' } );
		}
		else
			allData.push( { data: exposureData });
		
		var chart = new Highcharts.Chart({
			chart: { renderTo: 'container', type: 'line' },
            title: { text: null },
			legend: { enabled: true },
			credits: { enabled: false },
			//colors: ['#003366'],
			xAxis: 
			{ type: 'datetime', maxPadding: 0 },
			yAxis: 
			{ 
				title: { text: 'Exposure (mm)' },
				labels: { formatter: function() { return Highcharts.numberFormat(this.value / 1000000, 0); } }
				//min: 0,
				//maxPadding: 0
			},
			plotOptions: 
			{ series: { marker: { radius: 2 } } },
			series: allData,
			tooltip: { hideDelay: 3000, valueSuffix: '', valueDecimals: 0}
		});
	});
}

function downloadWithName(uri, name) {
    function eventFire(el, etype){
        if (el.fireEvent) {
            (el.fireEvent('on' + etype));
        } else {
            var evObj = document.createEvent('Events');
            evObj.initEvent(etype, true, false);
            el.dispatchEvent(evObj);
        }
    }
    var link = document.createElement("a");
    link.download = name;
    link.href = uri;
    eventFire(link, "click");
}
