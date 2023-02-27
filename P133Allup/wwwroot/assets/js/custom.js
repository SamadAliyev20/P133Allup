$(document).ready(function () {

    $(document).on('click', '.productCount', function () {
        let count = $(this).val();
        let productId = $(this).attr('data-productId');

        fetch('/Product/ChangeBasketProductCount/' + productId + '?count=' + count).then(res => {
            return res.text();
        }).then(data => {
            $(".productTable").html(data);
        })

    })

    $('.addToBasket').click(function myfunction(e) {
        e.preventDefault();

        let productId = $(this).data('id');

        fetch('basket/AddBasket?id=' + productId)
            .then(res => {
                return res.text();
            }).then(data => {
                $('.header-cart').html(data)

            })
    })

    $("#productModal").click(function (e) {
        e.preventDefault();

        let url = $(this).attr('href');
        

        fetch(url).then(res => {
            return res.text();
        })
            .then(data => {
                $('.modal-content').html(data);
                $('.quick-view-image').slick({
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    arrows: false,
                    dots: false,
                    fade: true,
                    asNavFor: '.quick-view-thumb',
                    speed: 400,
                });

                $('.quick-view-thumb').slick({
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    asNavFor: '.quick-view-image',
                    dots: false,
                    arrows: false,
                    focusOnSelect: true,
                    speed: 400,
                });
            })

    })

    $("#searchinput").keyup(function () {
        let search = $(this).val();
        let category = $("#category").find(":selected").val();

        if (search.length >= 3) {
            fetch('product/search?search=' + search + '&categoryId=' + category)
                .then(res => {
                    return res.text()
                }).then(data => {
                    $('#searchbody').html(data)
                })
        }
        else {
            $('#searchbody').html('')
        }
    })
})