namespace Library81.Client.wwwroot.js
{
    public class book3d
    {
        // wwwroot/js/book3d.js
// Livre 3D Babylon.js – procédural + animations
(function () {
        const ease = () => {
            const e = new BABYLON.CubicEase();
            e.setEasingMode(BABYLON.EasingFunction.EASINGMODE_EASEINOUT);
            return e;
        };

        function deg(rad) { return rad * 180 / Math.PI; }

        // Petite fabrique d’animations
        function animateRotationY(mesh, from, to, fps = 60, durationMs = 700) {
            const frames = Math.round((durationMs / 1000) * fps);
            const anim = new BABYLON.Animation(
                `rotY_${Date.now()}`, "rotation.y", fps,
                BABYLON.Animation.ANIMATIONTYPE_FLOAT,
                BABYLON.Animation.ANIMATIONLOOPMODE_CONSTANT
            );
            anim.setKeys([{ frame: 0, value: from }, { frame: frames, value: to }]);
            anim.setEasingFunction(ease());
            mesh.animations = mesh.animations || [];
            mesh.animations.push(anim);
            return anim;
        }

        function animatePosition(mesh, from, to, fps = 60, durationMs = 700) {
            const frames = Math.round((durationMs / 1000) * fps);
            const anim = new BABYLON.Animation(
                `pos_${Date.now()}`, "position", fps,
                BABYLON.Animation.ANIMATIONTYPE_VECTOR3,
                BABYLON.Animation.ANIMATIONLOOPMODE_CONSTANT
            );
            anim.setKeys([{ frame: 0, value: from }, { frame: frames, value: to }]);
            anim.setEasingFunction(ease());
            mesh.animations = mesh.animations || [];
            mesh.animations.push(anim);
            return anim;
        }

        // Création du livre
        function createBook(scene, options = {}) {
            const {
                width = 0.21,  // 21 cm
                height = 0.297, // 29.7 cm
                thickness = 0.03, // 3 cm total
                pageCount = 24,   // nombre de pages animables (recto-verso = 12 feuilles)
                coverThickness = 0.004,
                pageThickness = 0.0006,
                pageTint = new BABYLON.Color3(0.98, 0.97, 0.94),
                coverColor = new BABYLON.Color3(0.24, 0.16, 0.12),
                spineWidth = 0.016
            } = options;

            const book = new BABYLON.TransformNode("book", scene);

            // Matériaux
            const matCover = new BABYLON.PBRMaterial("matCover", scene);
            matCover.roughness = 0.6; matCover.metallic = 0.0; matCover.albedoColor = coverColor;

            const matPage = new BABYLON.PBRMaterial("matPage", scene);
            matPage.albedoColor = pageTint; matPage.roughness = 0.9; matPage.metallic = 0.0;

            const matSpine = new BABYLON.PBRMaterial("matSpine", scene);
            matSpine.albedoColor = coverColor.scale(0.9);
            matSpine.roughness = 0.55; matSpine.metallic = 0.0;

            // DOS
            const spine = BABYLON.MeshBuilder.CreateBox("spine", {
                width: spineWidth,
                height: height,
                depth: thickness
            }, scene);
            spine.material = matSpine;
            spine.parent = book;
            spine.position.x = -width / 2 + spineWidth / 2;

            // COUVERTURE GAUCHE (fixe)
            const coverL = BABYLON.MeshBuilder.CreateBox("coverL", {
                width: (width - spineWidth) / 2,
                height: height,
                depth: coverThickness
            }, scene);
            coverL.material = matCover;
            coverL.parent = book;
            // Ajuster pour qu’elle soit au ras du dos, côté gauche
            coverL.position.x = -spineWidth / 2 - coverL._width / 2;
            coverL.position.z = -thickness / 2 + coverThickness / 2;

            // PIVOT pour COUVERTURE DROITE (animée)
            const coverR_pivot = new BABYLON.TransformNode("coverR_pivot", scene);
            coverR_pivot.parent = book;
            // charnière au bord du dos (côté droit du dos)
            coverR_pivot.position = new BABYLON.Vector3(-spineWidth / 2, 0, -thickness / 2 + coverThickness / 2);

            const coverR = BABYLON.MeshBuilder.CreateBox("coverR", {
                width: (width - spineWidth) / 2,
                height: height,
                depth: coverThickness
            }, scene);
            coverR.material = matCover;
            coverR.parent = coverR_pivot;
            // Décaler pour que sa charnière soit sur le pivot
            coverR.position.x = coverR._width / 2;

            // BLOC PAGES (empilage)
            const pagesRoot = new BABYLON.TransformNode("pagesRoot", scene);
            pagesRoot.parent = book;
            pagesRoot.position.z = -thickness / 2 + coverThickness + (pageThickness * pageCount) / 2;

            // Créons un léger “ventre” de pages via une courbe sinusoïdale (position Y)
            const leafCount = Math.floor(pageCount / 2);
            const pagesRightPivot = []; // pivots des pages qui se tournent vers la gauche

            for (let i = 0; i < pageCount; i++) {
                const pagePivot = new BABYLON.TransformNode(`pagePivot_${i}`, scene);
                pagePivot.parent = pagesRoot;

                // Position Z dans l’épaisseur
                const z = (i * pageThickness) - (pageCount * pageThickness) / 2;
                pagePivot.position = new BABYLON.Vector3(0, 0, z);

                // Page (moitié droite qui se tourne individuellement autour du dos)
                const page = BABYLON.MeshBuilder.CreateBox(`page_${i}`, {
                    width: (width - spineWidth),   // double page à plat
                    height: height,
                    depth: pageThickness
                }, scene);
                page.material = matPage;
                page.parent = pagePivot;

                // Par défaut : la page est centrée. On va placer le pivot sur la ligne du dos (charnière),
                // donc on décale la géométrie pour tourner autour du bord gauche.
                page.position.x = (width - spineWidth) / 2 - 0.00001; // léger epsilon pour éviter Z-fighting

                // On ne tourne QUE les pages de droite (visuellement) — on simulera le côté gauche par l’empilement.
                // Inclinaison subtile pour un aspect réaliste :
                const belly = 0.003 * Math.sin((i / pageCount) * Math.PI);
                pagePivot.rotation.z = belly;

                // On stocke le pivot pour animer les pages comme un flip (rotationY autour du dos)
                pagesRightPivot.push(pagePivot);
            }

            // Réglage caméra/lumières
            const camera = new BABYLON.ArcRotateCamera(
                "cam", BABYLON.Tools.ToRadians(-35), BABYLON.Tools.ToRadians(65), 0.8, BABYLON.Vector3.Zero(), scene
            );
            camera.attachControl(scene.getEngine().getRenderingCanvas(), true);

            // Lumières
            const hemi = new BABYLON.HemisphericLight("hemi", new BABYLON.Vector3(0, 1, 0), scene);
            hemi.intensity = 0.9;
            const dir = new BABYLON.DirectionalLight("dir", new BABYLON.Vector3(-0.3, -1, 0.3), scene);
            dir.position = new BABYLON.Vector3(0.6, 0.8, -0.6);
            dir.intensity = 0.6;

            // Ombres douces
            const shadowGen = new BABYLON.ShadowGenerator(1024, dir);
            shadowGen.useExponentialShadowMap = true;
            [spine, coverL, coverR].forEach(m => shadowGen.addShadowCaster(m));
            scene.environmentIntensity = 0.7;
            scene.createDefaultEnvironment({ createSkybox: false, createGround: true, groundSize: 2.0 });

            // État du livre
            let isOpen = false;
            let currentPage = 0; // combien de pages ont été tournées (0..pageCount)

            // API : Ouvrir / Fermer (couvre droite)
            const open = (duration = 700) => {
                if (isOpen) return scene;
                coverR_pivot.animations = [];
                const start = coverR_pivot.rotation.y;
                const end = -Math.PI; // ouvrir vers la gauche
                const anim = animateRotationY(coverR_pivot, start, end, 60, duration);
                scene.beginAnimation(coverR_pivot, 0, 60, false);
                isOpen = true;
                return scene;
            };

            const close = (duration = 700) => {
                if (!isOpen) return scene;
                // Ramener toutes les pages au repos:
                for (let i = currentPage - 1; i >= 0; i--) {
                    const p = pagesRightPivot[i];
                    p.animations = [];
                    const anim = animateRotationY(p, p.rotation.y, 0, 60, duration * 0.6);
                    scene.beginAnimation(p, 0, 60, false);
                }
                currentPage = 0;

                coverR_pivot.animations = [];
                const start = coverR_pivot.rotation.y;
                const end = 0; // refermer
                const anim = animateRotationY(coverR_pivot, start, end, 60, duration);
                scene.beginAnimation(coverR_pivot, 0, 60, false);
                isOpen = false;
                return scene;
            };

            // API : Page suivante / précédente
            const nextPage = (duration = 500) => {
                if (!isOpen || currentPage >= pageCount) return false;
                const p = pagesRightPivot[currentPage];
                if (!p) return false;
                p.animations = [];
                const anim = animateRotationY(p, p.rotation.y, -Math.PI, 60, duration);
                scene.beginAnimation(p, 0, 60, false);
                currentPage++;
                return true;
            };

            const prevPage = (duration = 500) => {
                if (!isOpen || currentPage <= 0) return false;
                const i = currentPage - 1;
                const p = pagesRightPivot[i];
                p.animations = [];
                const anim = animateRotationY(p, p.rotation.y, 0, 60, duration);
                scene.beginAnimation(p, 0, 60, false);
                currentPage--;
                return true;
            };

            // Rendu doux
            scene.onBeforeRenderObservable.add(() => { /* hook si besoin */ });

            return {
                root: book,
                open, close, nextPage, prevPage,
                get state() { return { isOpen, currentPage, pageCount }; }
            };
        }

        // Expose une fonction globale pour Blazor
        window.Book3D = {
            init: function (canvasId, opts) {
                const canvas = document.getElementById(canvasId);
                const engine = new BABYLON.Engine(canvas, true, { preserveDrawingBuffer: true, stencil: true });
                const scene = new BABYLON.Scene(engine);
                scene.clearColor = new BABYLON.Color4(0, 0, 0, 0); // fond transparent

                const book = createBook(scene, opts || {});

                engine.runRenderLoop(() => scene.render());
                window.addEventListener("resize", () => engine.resize());

                // Retourner une petite API utilisable depuis Blazor
                return {
                    open: () => book.open(),
                    close: () => book.close(),
                    nextPage: () => book.nextPage(),
                    prevPage: () => book.prevPage(),
                    getState: () => book.state
                };
            }
        };
    })();

    }
}
