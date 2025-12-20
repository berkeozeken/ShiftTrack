$(document).ready(function () {
    $(".btn-delete").on("click", function () {
        var id = $(this).data("id");

        if (!confirm("Bu mesai tanýmýný silmek istiyor musunuz?")) {
            return;
        }

        $.ajax({
            url: "/ShiftDefinitions/Delete",
            type: "POST",
            data: { id: id },
            success: function () {
                $("#row-" + id).remove();
            },
            error: function () {
                alert("Silme iþlemi sýrasýnda hata oluþtu.");
            }
        });
    });
});
