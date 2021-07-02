
$(document).ready(function(){
    'use strict';
        
});

function Main_Rule(){
    /*
     Favorite Item [ select and unselect favorite items]
    */
    var IconFav = $('i[data-favorite="IconFav"]');
    IconFav.click(function(){
        $(this).toggleClass('active');
    });
    
    /*
     Open and Close Panel contain plus icon
    */
    var IconOpenClose = $('.panel .panel-heading i[data-toggle="OpenClosePanel"]');
    IconOpenClose.click(function(){
       $(this).parent().next().fadeToggle(150);
       if($(this).hasClass('fa-plus')){
        $(this).removeClass('fa-plus');
        $(this).addClass('fa-minus');
       }else{
        $(this).removeClass('fa-minus');
        $(this).addClass('fa-plus');
       }
    });
    
    /*
     Confirm Message on Click button
    */
    var ConfirmMsg = $('.confirm');
    ConfirmMsg.click(function(){
       return confirm('Are You Sure ?'); 
    });
    
    /*
     Switch radio button when click
    */
    var RadioBtnChange = $('.radio-btn-change .btn');
    RadioBtnChange.click(function(){
       $(this).addClass('active').siblings().removeClass('active');
    });
    
}

function Show_User_Error_Message(element){
    var alt = element + " div[id = 'user-error-msg']";
    $(alt).each(function(){
        var Msg = $(this).children('span');
        if($(Msg).is(':empty')){
            $(this).css('display','none');
        }else{
            $(this).css('display','block');
        }
    });
}

$(function(){
    
    'use strict';
    
    /* Start Main Rule */
    
    Main_Rule();
    
    /* End Main Rule */
    
    /* Start User Sign In, Sign Up */
    Show_User_Error_Message('.user-signin');
    Show_User_Error_Message('.user-signup');
    
    /* End User Sign In, Sign Up */
    
    /* Start Group Page */
    Show_User_Error_Message('.create-group-page');
    Show_User_Error_Message('.edit-group-page');
    /* End Group Page */
    
    
});