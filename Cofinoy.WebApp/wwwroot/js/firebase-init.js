import { initializeApp } from "https://www.gstatic.com/firebasejs/12.2.1/firebase-app.js";
import { getAnalytics } from "https://www.gstatic.com/firebasejs/12.2.1/firebase-analytics.js";
import { getFirestore } from "https://www.gstatic.com/firebasejs/12.2.1/firebase-firestore.js";
import * as firestore from "https://www.gstatic.com/firebasejs/12.2.1/firebase-firestore.js";
import { getStorage, ref, uploadBytes, getDownloadURL, deleteObject } from "https://www.gstatic.com/firebasejs/12.2.1/firebase-storage.js";


export const firebaseConfig = {
     apiKey: "AIzaSyAo9JrgoHPqmsz9C79NL9xpo5As4IbAAAM",
      authDomain: "cofinoy-5c26c.firebaseapp.com",
      projectId: "cofinoy-5c26c",
      storageBucket: "cofinoy-5c26c.firebasestorage.app",
      messagingSenderId: "635740452795",
      appId: "1:635740452795:web:6a8716330f68662935f69e",
      measurementId: "G-9LKJHHVDPH"
};

const app = initializeApp(firebaseConfig);
const db = getFirestore(app);
const analytics = getAnalytics(app);
const storage = getStorage(app);

export { app, db, analytics, firestore, storage, ref, uploadBytes, getDownloadURL, deleteObject };
