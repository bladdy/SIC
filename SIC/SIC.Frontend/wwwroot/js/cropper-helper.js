let cropper;

window.previewImageFromInput = () => {
    const input = document.getElementById("fileInput");
    const image = document.getElementById("preview");

    if (!input || !input.files || input.files.length === 0) return;

    const file = input.files[0];
    const url = URL.createObjectURL(file);

    image.src = url;
};

window.initCropper = () => {
    const image = document.getElementById("preview");
    if (!image || !image.src) return;

    if (cropper) {
        cropper.destroy();
    }

    cropper = new Cropper(image, {
        //aspectRatio: 3 / 1,
        viewMode: 1,
        dragMode: 'move',
        autoCropArea: 1,
        background: false
    });
};

window.getCropData = () => {
    if (!cropper) return null;
    return cropper.getData(true);
};

window.previewImageFromInput = () => {
    const input = document.querySelector('input[type="file"]');
    const file = input.files[0];

    if (!file) return;

    const url = URL.createObjectURL(file);
    document.getElementById("preview").src = url;
};