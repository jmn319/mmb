
// ******************************** VISITS  ********************************************
$(function () {
    var datasets = {
        "europe": {
            label: "Europe",
            data: [[1988, 290], [1989, 300], [1990, 450], [1991, 303], [1992, 420], [1993, 110], [1994, 250], [1995, 640], [1996, 290], [1997, 280], [1998, 380], [1999, 343], [2000, 443], [2001, 332], [2002, 233], [2003, 444], [2004, 454], [2005, 333], [2011, 343]]
        },        
        "united": {
            label: "United Stats",
            data: [[1988, 432], [1989, 54], [1990, 644], [1992, 433], [1993, 566], [1994, 664], [1995, 555], [1996, 455], [1997, 433], [1998, 344], [1999, 322], [2000, 234], [2001, 321], [2002, 445], [2003, 332], [2004, 653], [2005, 554], [2011, 221]]
        },
        "africa": {
            label: "Africa",
            data: [[1988, 754], [1989, 134], [1990, 555], [1991, 333], [1992, 433], [1993, 433], [1994, 455], [1995, 665], [1996, 333], [1997, 133], [1998, 344], [1999, 433], [2000, 556], [2001, 137], [2002, 345], [2003, 643], [2004, 765], [2005, 345], [2011, 543]]
        },
        "asie": {
            label: "Asie",
            data: [[1988, 454], [1989, 363], [1990, 746], [1991, 144], [1992, 233], [1993, 233], [1994, 344], [1995, 455], [1996, 566], [1997, 444], [1998, 142], [1999, 743], [2000, 464], [2001, 647], [2002, 322], [2003, 322], [2004, 432], [2005, 464], [2011, 765]]
        }
      
    };

    // hard-code color indices to prevent them from shifting as
    // countries are turned on/off
    var i = 0;
    $.each(datasets, function(key, val) {
        val.color = i;
        ++i;
    });
    
    // insert checkboxes 
    var choiceContainer = $("#choices");
    $.each(datasets, function(key, val) {
        choiceContainer.append('<input type="checkbox" style="float:left;margin-left:5px" name="' + key +
                               '" checked="checked" id="id' + key + '">' +
                               '<label style="float:left; margin:0 15px 5px; color:#777777;" for="id' + key + '">'
                                + val.label + '</label>');
    });
    choiceContainer.find("input").click(plotAccordingToChoices);

    
    function plotAccordingToChoices() {
        var data = [];

        choiceContainer.find("input:checked").each(function () {
            var key = $(this).attr("name");
            if (key && datasets[key])
                data.push(datasets[key]);
        });

        if (data.length > 0)
            $.plot($("#placeholder"), data, {
                yaxis: { min: 0, },
                xaxis: { tickDecimals: 0 }
            });
    }

    plotAccordingToChoices();


// *********************************** Earning *******************************************

  $(function () {

    // data
    var data = [
        { label: "Project 1",  data: 50},
        { label: "Project 2",  data: 30},
        { label: "Project 3",  data: 90},
        { label: "Project 4",  data: 70},
        { label: "Project 5",  data: 80},
        { label: "Project 6",  data: 110}
    ];
    /*var data = [
        { label: "Series1",  data: [[1,10]]},
        { label: "Series2",  data: [[1,30]]},
        { label: "Series3",  data: [[1,90]]},
        { label: "Series4",  data: [[1,70]]},
        { label: "Series5",  data: [[1,80]]},
        { label: "Series6",  data: [[1,0]]}
    ];*/
  /*  var data = [];
    var series = Math.floor(Math.random()*10)+1;
    for( var i = 0; i<series; i++)
    {
        data[i] = { label: "200"+(i+1), data: Math.floor(Math.random()*100)+1 }
    } */
       $.plot($("#earning"), data,
{
        series: {
            pie: {
                show: true,
                radius: 1,
                label: {
                    show: true,
                    radius: 2/3,
                    formatter: function(label, series){
                        return '<div style="font-size:8pt;text-align:center;padding:2px;color:white;">'+label+'<br/>'+Math.round(series.percent)+'%</div>';
                    },
                    threshold: 0.1
                }
            }
        },
        legend: {
            show: false
        }
});
    
    });

});

