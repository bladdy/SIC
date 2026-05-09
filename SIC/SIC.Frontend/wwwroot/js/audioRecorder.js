window.audioRecorder = {
    mediaRecorder: null,
    audioChunks: [],

    start: async function () {

        const stream =
            await navigator.mediaDevices.getUserMedia({
                audio: true
            });

        this.audioChunks = [];

        this.mediaRecorder =
            new MediaRecorder(stream);

        this.mediaRecorder.ondataavailable = (event) => {

            if (event.data.size > 0) {
                this.audioChunks.push(event.data);
            }
        };

        this.mediaRecorder.start();
    },

    stop: async function () {

        return new Promise((resolve) => {

            this.mediaRecorder.onstop = async () => {

                const audioBlob = new Blob(
                    this.audioChunks,
                    { type: 'audio/webm' }
                );

                const fileName =
                    `audio_${Date.now()}.webm`;

                // 🔹 Convertir a Base64
                const reader = new FileReader();

                reader.onloadend = () => {

                    const base64 =
                        reader.result.split(',')[1];

                    resolve({
                        fileName: fileName,
                        contentType: 'audio/webm',
                        base64Data: base64
                    });
                };

                reader.readAsDataURL(audioBlob);
            };

            this.mediaRecorder.stop();
        });
    }
};