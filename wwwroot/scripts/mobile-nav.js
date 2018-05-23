
$(document).ready(function () {

    //Mobile Nav
    $(".mobile-nav").click(function () {
        if ($(this).hasClass("js-active")) {
            $(this).removeClass("js-active");
            $(".navigation").removeClass("js-active");
        }
        else {
            $(this).addClass("js-active");
            $(".navigation").addClass("js-active");
        }
    });

    //Click event of mobile menu
    $(".navigation .nav-item").click(function () {
        $(".mobile-nav").removeClass("js-active");
        $(".navigation").removeClass("js-active");
    });
});