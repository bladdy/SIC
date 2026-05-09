window.audioRecorder = {

    mediaRecorder: null,
    audioChunks: [],
    mimeType: null,

    start: async function () {

        const stream =
            await navigator.mediaDevices.getUserMedia({
                audio: true
            });

        this.audioChunks = [];

        // 🔹 Detectar formato compatible
        if (MediaRecorder.isTypeSupported('audio/mp4')) {

            this.mimeType = 'audio/mp4';

        } else if (
            MediaRecorder.isTypeSupported('audio/webm')
        ) {

            this.mimeType = 'audio/webm';

        } else {

            this.mimeType = '';
        }

        this.mediaRecorder =
            new MediaRecorder(stream, {
                mimeType: this.mimeType
            });

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
                    {
                        type: this.mimeType
                    });

                // 🔹 Extensión dinámica
                let extension = "webm";

                if (this.mimeType.includes("mp4")) {
                    extension = "m4a";
                }

                const fileName =
                    `audio_${Date.now()}.${extension}`;

                const reader = new FileReader();

                reader.onloadend = () => {

                    const base64 =
                        reader.result.split(',')[1];

                    resolve({
                        fileName: fileName,
                        contentType: this.mimeType,
                        base64Data: base64
                    });
                };

                reader.readAsDataURL(audioBlob);
            };

            this.mediaRecorder.stop();
        });
    }
};