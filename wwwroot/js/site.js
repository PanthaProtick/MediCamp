/**
 * MediCamp Core Client-Side Engine
 * Features:
 * 1. Dual Theme System: Dark Mode (Default) & White (Light) Mode with instant toggle
 * 2. Instant Bilingual Translation (English / Bangla) with Anek Bangla & Inter fonts
 * 3. LocalStorage persistence for both theme and language preferences
 */

(function () {
  'use strict';

  // Comprehensive Bilingual Dictionary
  const translations = {
    en: {
      // Navbar & Brand
      'brand.title': 'MediCamp',
      'brand.tagline': 'Healthcare Camps Bangladesh',
      'nav.home': 'Home',
      'nav.findCamps': 'Find Camps',
      'nav.userManagement': 'User Management',
      'nav.mission': 'Mission & SDG 3',
      'nav.signIn': 'Sign In',
      'nav.register': 'Register Account',
      'nav.selectRole': 'Select Role to Register',
      'nav.rolePatient': 'Patient Registration',
      'nav.roleDoctor': 'Doctor Registration',
      'nav.roleVolunteer': 'Volunteer Registration',
      'nav.rolePharmacist': 'Pharmacist Registration',
      'nav.roleHost': 'Organizing NGO Host',
      'nav.signOut': 'Sign Out',
      'theme.toLight': 'Switch to White Theme',
      'theme.toDark': 'Switch to Dark Mode',

      // Hero Section
      'hero.badge': 'SDG 3 & 10 Aligned • 100% Free Rural Healthcare OS',
      'hero.title': 'Smart Healthcare Camps.',
      'hero.titlePre': 'Smart Healthcare Camps.',
      'hero.titleHighlight': '',
      'hero.desc': 'Orchestrating NGO medical outreach across rural Bangladesh.',
      'hero.btnCamps': 'Find Upcoming Camps',
      'hero.btnHost': 'Register as Host NGO',
      'hero.btnBlood': 'Join Blood Donors',

      // Live Status Card
      'status.live': 'LIVE SYSTEM STATUS',
      'status.synced': 'Synced Live',
      'status.campTitle': 'Kurigram Char Camp Active',
      'status.campSub': 'Host: Friendship Bangladesh Healthcare • Venue: Rowmari',
      'status.queueTitle': "Today's Patient Queue",
      'status.queueCount': '492 / 650 Served',
      'status.avgWait': 'Avg Wait Time:',
      'status.avgWaitVal': '22 mins',
      'status.stockOut': 'Stock Outages:',
      'status.stockOutVal': '0 reported',
      'status.donorsCount': '18,400+',
      'status.donorsLabel': 'Blood Donors Ready',
      'status.triageSpeed': '< 45s',
      'status.triageLabel': 'Triage Intake Speed',
      'status.referralLabel': 'Hospital Referral:',
      'status.referralText': '2 critical patients routed to Rangpur Sadar Hospital.',

      // 6 Roles Section
      'roles.badge': 'Role-Based Ecosystem',
      'roles.heading': 'One Platform, Six Specialized Roles',
      'roles.subheading': 'MediCamp provides each healthcare participant with a focused dashboard optimized for low-bandwidth field deployment.',
      'roles.r1Title': 'System Administrator',
      'roles.r1Desc': 'Controls global user accounts, approves NGO Host registrations, maintains master medicine lists, and generates national reports.',
      'roles.r1Btn': 'Sign In as Admin',
      'roles.r2Title': 'Host (Organizing NGO)',
      'roles.r2Desc': 'Plans camps with budgets, assigns doctors and volunteers, allocates medicine stock, and monitors field expenses.',
      'roles.r2Btn': 'Register NGO Host',
      'roles.r3Title': 'Volunteer Doctor',
      'roles.r3Desc': 'Reviews patient medical history across prior camps, inputs diagnoses, builds e-prescriptions, and creates hospital referrals.',
      'roles.r3Btn': 'Register as Doctor',
      'roles.r4Title': 'Field Volunteer',
      'roles.r4Desc': 'Searches or registers patients by NID, records vital signs (BP, BMI, Temp), issues queue tokens, and logs follow-up visits.',
      'roles.r4Btn': 'Register as Volunteer',
      'roles.r5Title': 'Camp Pharmacist',
      'roles.r5Desc': 'Receives live prescription queues from doctor workspaces, dispenses medicines with stock auto-deduction, and requests restocks.',
      'roles.r5Btn': 'Register as Pharmacist',
      'roles.r6Title': 'Registered Patient',
      'roles.r6Desc': 'Views upcoming camp schedules, accesses digital health records, and opts into the community Blood Donor Registry.',
      'roles.r6Btn': 'Register Patient Account',

      // Blood Donor Hub
      'blood.badge': 'Integrated Blood Donor Network',
      'blood.heading': 'Voluntary Blood Donation Hub',
      'blood.desc': 'Patients can opt-in as voluntary blood donors during registration. Search donors by blood group (A+, B+, O+, AB-) and district for emergency hospital referrals.',
      'blood.btn': 'Opt-in as Donor Now',

      // Camps Preview Section
      'camps.heading': 'Upcoming Community Health Camps',
      'camps.subheading': 'Free medical consultations and medicines scheduled in rural upazilas.',
      'camps.btnBrowseAll': 'Browse All Camps',
      'camps.filterSearch': 'Search by camp title, venue, or NGO...',
      'camps.filterAllDistricts': 'All Districts',
      'camps.filterAllTypes': 'All Camp Types',
      'camps.btnSearch': 'Search',
      'camps.noCampsFound': 'No Healthcare Camps Found',
      'camps.noCampsSub': 'Try changing your district or category search filters.',
      'camps.registerPatient': 'Register Patient',
      'camps.expected': 'Expected:',
      'camps.patients': 'Patients',

      // Login & Auth
      'auth.signInTitle': 'Portal Sign In',
      'auth.signInDesc': 'Enter your registered Email Address or National ID (NID)',
      'auth.idLabel': 'Email Address or National ID (NID) *',
      'auth.idPlaceholder': 'e.g. email@domain.org or NID number',
      'auth.passwordLabel': 'Password *',
      'auth.rememberMe': 'Keep me signed in',
      'auth.btnSignIn': 'Sign In to Account',
      'auth.notRegistered': "Haven't registered an account yet?",
      'auth.btnRegisterFirst': 'Register New Account First',

      // Footer
      'footer.desc': 'A role-based Community Healthcare Camp Management System designed to empower rural and underserved communities in Bangladesh with unified patient records, camp coordination, and reliable healthcare delivery.',
      'footer.sdg3': 'UN SDG 3: Good Health',
      'footer.sdg10': 'UN SDG 10: Reduced Inequalities',
      'footer.quickAccess': 'Phase 1 Quick Access',
      'footer.impactTitle': 'Community Healthcare Camp Impact',
      'footer.impactDesc': 'Covering 6 distinct actors: System Admins, Organizing NGO Hosts, Volunteer Clinicians, Field Screening Volunteers, Dispensary Pharmacists, and Rural Patients.',
      'footer.emergencyDesk': 'Emergency Camp Coordination Desk',
      'footer.supportContact': 'Email: support@medicamp.org | Helpline: 16263',
      'footer.copyright': '© 2026 MediCamp Bangladesh. All rights reserved. Department of CSE.',
      'footer.linkArchitecture': 'About Architecture',
      'footer.linkDirectory': 'Public Camps Directory'
    },
    bn: {
      // Navbar & Brand
      'brand.title': 'মেডিক্যাম্প',
      'brand.tagline': 'হেলথকেয়ার ক্যাম্প বাংলাদেশ',
      'nav.home': 'হোম',
      'nav.findCamps': 'ক্যাম্প খুঁজুন',
      'nav.userManagement': 'ব্যবহারকারী ব্যবস্থাপনা',
      'nav.mission': 'মিশন ও এসডিজি ৩',
      'nav.signIn': 'লগইন করুন',
      'nav.register': 'অ্যাকাউন্ট নিবন্ধন',
      'nav.selectRole': 'নিবন্ধনের ভূমিকা নির্বাচন করুন',
      'nav.rolePatient': 'রোগী নিবন্ধন',
      'nav.roleDoctor': 'চিকিৎসক নিবন্ধন',
      'nav.roleVolunteer': 'স্বেচ্ছাসেবক নিবন্ধন',
      'nav.rolePharmacist': 'ফার্মাসিস্ট নিবন্ধন',
      'nav.roleHost': 'আয়োজক এনজিও (হোস্ট)',
      'nav.signOut': 'লগআউট',
      'theme.toLight': 'সাদা (লাইট) থিমে পরিবর্তন করুন',
      'theme.toDark': 'ডার্ক মোডে পরিবর্তন করুন',

      // Hero Section
      'hero.badge': 'এসডিজি ৩ ও ১০ সমর্থিত • ১০০% বিনামূল্যে গ্রামীণ স্বাস্থ্যসেবা ওএস',
      'hero.title': 'স্মার্ট হেলথকেয়ার ক্যাম্প।',
      'hero.titlePre': 'স্মার্ট হেলথকেয়ার ক্যাম্প।',
      'hero.titleHighlight': '',
      'hero.desc': 'গ্রামীণ বাংলাদেশে এনজিও স্বাস্থ্যসেবা কার্যক্রম পরিচালনা।',
      'hero.btnCamps': 'আসন্ন ক্যাম্পসমূহ দেখুন',
      'hero.btnHost': 'এনজিও হোস্ট হিসেবে যুক্ত হোন',
      'hero.btnBlood': 'রক্তদাতা হিসেবে যুক্ত হোন',

      // Live Status Card
      'status.live': 'সরাসরি সিস্টেম স্ট্যাটাস',
      'status.synced': 'লাইভ সিঙ্ক হচ্ছে',
      'status.campTitle': 'কুড়িগ্রাম চর ক্যাম্প চলমান',
      'status.campSub': 'হোস্ট: ফ্রেন্ডশিপ বাংলাদেশ হেলথকেয়ার • স্থান: রৌমারী',
      'status.queueTitle': 'আজকের রোগীর সারি',
      'status.queueCount': '৪৯২ / ৬৫০ সেবা সম্পন্ন',
      'status.avgWait': 'গড় অপেক্ষার সময়:',
      'status.avgWaitVal': '২২ মিনিট',
      'status.stockOut': 'ওষুধ ঘাটতি:',
      'status.stockOutVal': '০ টি রিপোর্ট',
      'status.donorsCount': '১৮,৪০০+',
      'status.donorsLabel': 'প্রস্তুত স্বেচ্ছাসেবী রক্তদাতা',
      'status.triageSpeed': '< ৪৫ সে.',
      'status.triageLabel': 'প্রাথমিক চেকআপের গতি',
      'status.referralLabel': 'জরুরি হাসপাতাল রেফারেল:',
      'status.referralText': '২ জন আশঙ্কাজনক রোগীকে রংপুর সদর হাসপাতালে স্থানান্তর করা হয়েছে।',

      // 6 Roles Section
      'roles.badge': 'ভূমিকা-ভিত্তিক ইকোসিস্টেম',
      'roles.heading': 'একটি সমন্বিত প্ল্যাটফর্ম, ছয়টি বিশেষায়িত ভূমিকা',
      'roles.subheading': 'মেডিক্যাম্প প্রতিটি স্বাস্থ্যকর্মীর জন্য কম ব্যান্ডউইথে কাজ করার উপযোগী একটি দ্রুত ও আধুনিক ড্যাশবোর্ড নিশ্চিত করে।',
      'roles.r1Title': 'সিস্টেম অ্যাডমিনিস্ট্রেটর',
      'roles.r1Desc': 'কেন্দ্রীয় ব্যবহারকারী অ্যাকাউন্ট নিয়ন্ত্রণ, এনজিও অনুমোদন, ওষুধের মাস্টার তালিকা সংরক্ষণ এবং জাতীয় রিপোর্ট তৈরি করেন।',
      'roles.r1Btn': 'অ্যাডমিন হিসেবে প্রবেশ করুন',
      'roles.r2Title': 'হোস্ট (আয়োজক এনজিও)',
      'roles.r2Desc': 'বাজেটসহ ক্যাম্প পরিকল্পনা, চিকিৎসক ও স্বেচ্ছাসেবক বরাদ্দ, ওষুধ সরবরাহ বণ্টন এবং ব্যয়ের হিসাব পর্যবেক্ষণ করেন।',
      'roles.r2Btn': 'এনজিও হোস্ট নিবন্ধন',
      'roles.r3Title': 'স্বেচ্ছাসেবী চিকিৎসক',
      'roles.r3Desc': 'রোগীর পূর্ববর্তী মেডিকেল ইতিহাস পর্যালোচনা, রোগ নির্ণয়, ডিজিটাল ই-প্রেসক্রিপশন প্রদান ও হাসপাতাল রেফারেল লেখেন।',
      'roles.r3Btn': 'চিকিৎসক হিসেবে নিবন্ধন',
      'roles.r4Title': 'মাঠপর্যায়ের স্বেচ্ছাসেবক',
      'roles.r4Desc': 'এনআইডি দিয়ে রোগী তল্লাশি বা নতুন নিবন্ধন, ভাইটাল সাইন (রক্তচাপ, বিএমআই, তাপমাত্রা) পরিমাপ এবং টোকেন বিতরণ করেন।',
      'roles.r4Btn': 'স্বেচ্ছাসেবক নিবন্ধন',
      'roles.r5Title': 'ক্যাম্প ফার্মাসিস্ট',
      'roles.r5Desc': 'ডাক্তারের প্রেসক্রিপশন সরাসরি স্ক্রিনে পেয়ে ওষুধ বিতরণ করেন এবং স্টক থেকে স্বয়ংক্রিয়ভাবে বিয়োগ হয়।',
      'roles.r5Btn': 'ফার্মাসিস্ট নিবন্ধন',
      'roles.r6Title': 'নিবন্ধিত রোগী',
      'roles.r6Desc': 'আসন্ন স্বাস্থ্য ক্যাম্পের সময়সূচি দেখেন, ডিজিটাল প্রেসক্রিপশন সংগ্রহ করেন এবং স্বেচ্ছায় রক্তদাতা নেটওয়ার্কে যোগ দেন।',
      'roles.r6Btn': 'রোগীর অ্যাকাউন্ট খুলুন',

      // Blood Donor Hub
      'blood.badge': 'সমন্বিত রক্তদাতা নেটওয়ার্ক',
      'blood.heading': 'স্বেচ্ছাসেবী রক্তদান কেন্দ্র',
      'blood.desc': 'রোগীরা নিবন্ধনের সময় স্বেচ্ছায় রক্তদাতা হিসেবে সম্মতি দিতে পারেন। রক্তের গ্রুপ (A+, B+, O+, AB-) ও জেলা অনুযায়ী জরুরি মুহূর্তে সরাসরি রক্তদাতা খুঁজে নিন।',
      'blood.btn': 'রক্তদাতা হিসেবে নিবন্ধন করুন',

      // Camps Preview Section
      'camps.heading': 'আসন্ন কমিউনিটি স্বাস্থ্য ক্যাম্পসমূহ',
      'camps.subheading': 'গ্রামীণ উপজেলা ও চরাঞ্চলে বিনামূল্যে চিকিৎসাসেবা ও ওষুধ বিতরণ সূচি।',
      'camps.btnBrowseAll': 'সবগুলো ক্যাম্প দেখুন',
      'camps.filterSearch': 'ক্যাম্পের নাম, স্থান বা এনজিও দিয়ে খুঁজুন...',
      'camps.filterAllDistricts': 'সব জেলা',
      'camps.filterAllTypes': 'সব ধরণের ক্যাম্প',
      'camps.btnSearch': 'অনুসন্ধান',
      'camps.noCampsFound': 'কোনো স্বাস্থ্য ক্যাম্প পাওয়া যায়নি',
      'camps.noCampsSub': 'অনুগ্রহ করে জেলা বা ক্যাম্পের ধরণ পরিবর্তন করে পুনরায় চেষ্টা করুন।',
      'camps.registerPatient': 'রোগী নিবন্ধন করুন',
      'camps.expected': 'প্রত্যাশিত:',
      'camps.patients': 'রোগী',

      // Login & Auth
      'auth.signInTitle': 'পোর্টাল সাইন ইন',
      'auth.signInDesc': 'আপনার নিবন্ধিত ইমেইল ঠিকানা অথবা জাতীয় পরিচয়পত্র (NID) নম্বর দিন',
      'auth.idLabel': 'ইমেইল অথবা জাতীয় পরিচয়পত্র (NID) নম্বর *',
      'auth.idPlaceholder': 'যেমন: email@domain.org অথবা NID নম্বর',
      'auth.passwordLabel': 'পাসওয়ার্ড *',
      'auth.rememberMe': 'আমাকে মনে রাখুন',
      'auth.btnSignIn': 'অ্যাকাউন্টে প্রবেশ করুন',
      'auth.notRegistered': 'এখনও কোনো অ্যাকাউন্ট তৈরি করেননি?',
      'auth.btnRegisterFirst': 'প্রথমে নতুন অ্যাকাউন্ট নিবন্ধন করুন',

      // Footer
      'footer.desc': 'বাংলাদেশের গ্রামীণ ও দুর্গম জনপদের সাধারণ মানুষের কাছে মানসম্মত স্বাস্থ্যসেবা পৌঁছে দিতে একটি সমন্বিত ডিজিটাল কমিউনিটি ক্যাম্প ম্যানেজমেন্ট সিস্টেম।',
      'footer.sdg3': 'ইউএন এসডিজি ৩: সুস্বাস্থ্য ও সুস্থ জীবন',
      'footer.sdg10': 'ইউএন এসডিজি ১০: বৈষম্য হ্রাস',
      'footer.quickAccess': 'দ্রুত লিঙ্কসমূহ',
      'footer.impactTitle': 'কমিউনিটি স্বাস্থ্য ক্যাম্প প্রভাব',
      'footer.impactDesc': '৬টি বিশেষায়িত ভূমিকা: অ্যাডমিন, এনজিও হোস্ট, স্বেচ্ছাসেবী চিকিৎসক, মাঠ স্বেচ্ছাসেবক, ফার্মাসিস্ট এবং গ্রামীণ রোগী।',
      'footer.emergencyDesk': 'জরুরি ক্যাম্প সমন্বয় ডেস্ক',
      'footer.supportContact': 'ইমেইল: support@medicamp.org | হেল্পলাইন: ১৬২৬৩',
      'footer.copyright': '© ২০২৬ মেডিক্যাম্প বাংলাদেশ। সর্বস্বত্ব সংরক্ষিত। কম্পিউটার সায়েন্স অ্যান্ড ইঞ্জিনিয়ারিং বিভাগ।',
      'footer.linkArchitecture': 'সিস্টেম আর্কিটেকচার',
      'footer.linkDirectory': 'পাবলিক ক্যাম্প ডিরেক্টরি'
    }
  };

  /**
   * 1. Theme Management (Dark Mode & White/Light Theme)
   */
  window.setTheme = function (theme) {
    if (theme !== 'light' && theme !== 'dark') {
      theme = 'dark';
    }

    document.documentElement.setAttribute('data-theme', theme);
    if (document.body) {
      document.body.setAttribute('data-theme', theme);
    }
    try {
      localStorage.setItem('medicamp_theme', theme);
    } catch (e) {
      console.warn('localStorage not accessible:', e);
    }

    const toggleBtn = document.getElementById('themeToggleBtn');
    if (toggleBtn) {
      const currentLang = document.documentElement.lang || 'en';
      const dict = translations[currentLang] || translations.en;

      if (theme === 'light') {
        toggleBtn.innerHTML = '<i class="fa-solid fa-moon"></i>';
        toggleBtn.setAttribute('title', dict['theme.toDark'] || 'Switch to Dark Mode');
        toggleBtn.classList.add('active-light');
      } else {
        toggleBtn.innerHTML = '<i class="fa-solid fa-sun"></i>';
        toggleBtn.setAttribute('title', dict['theme.toLight'] || 'Switch to White Theme');
        toggleBtn.classList.remove('active-light');
      }
    }
  };

  window.toggleTheme = function () {
    const currentTheme = document.documentElement.getAttribute('data-theme') || 'dark';
    const nextTheme = currentTheme === 'light' ? 'dark' : 'light';
    window.setTheme(nextTheme);
  };

  /**
   * 2. Language Management (English & Bangla)
   */
  window.setLanguage = function (lang) {
    if (!translations[lang]) {
      lang = 'en';
    }

    try {
      localStorage.setItem('medicamp_lang', lang);
    } catch (e) {
      console.warn('localStorage not accessible:', e);
    }

    // Set document lang attribute and toggle bangla font styling on body
    document.documentElement.lang = lang;
    if (lang === 'bn') {
      document.body.classList.add('lang-bn');
    } else {
      document.body.classList.remove('lang-bn');
    }

    // Update active button indicator
    const btnEn = document.getElementById('btnLangEn');
    const btnBn = document.getElementById('btnLangBn');
    if (btnEn && btnBn) {
      if (lang === 'bn') {
        btnBn.classList.add('active');
        btnEn.classList.remove('active');
      } else {
        btnEn.classList.add('active');
        btnBn.classList.remove('active');
      }
    }

    // Update theme tooltip based on language
    const currentTheme = document.documentElement.getAttribute('data-theme') || 'dark';
    const toggleBtn = document.getElementById('themeToggleBtn');
    if (toggleBtn) {
      const dict = translations[lang];
      toggleBtn.setAttribute('title', currentTheme === 'light' ? dict['theme.toDark'] : dict['theme.toLight']);
    }

    // Update text for all elements with data-i18n
    const dict = translations[lang];
    const elements = document.querySelectorAll('[data-i18n]');
    elements.forEach(function (el) {
      const key = el.getAttribute('data-i18n');
      if (dict[key]) {
        const icon = el.querySelector('i');
        if (icon && el.childNodes.length > 1) {
          const iconClone = icon.cloneNode(true);
          el.innerHTML = '';
          el.appendChild(iconClone);
          el.appendChild(document.createTextNode(' ' + dict[key]));
        } else {
          el.textContent = dict[key];
        }
      }
    });

    // Update input placeholders
    const inputs = document.querySelectorAll('[data-i18n-placeholder]');
    inputs.forEach(function (input) {
      const key = input.getAttribute('data-i18n-placeholder');
      if (dict[key]) {
        input.setAttribute('placeholder', dict[key]);
      }
    });

    // Dispatch event
    const event = new CustomEvent('medicamp:languageChanged', { detail: { language: lang } });
    document.dispatchEvent(event);
  };

  /**
   * 3. Initialization on DOMContentLoaded
   */
  document.addEventListener('DOMContentLoaded', function () {
    // Restore Theme
    let savedTheme = 'dark';
    try {
      savedTheme = localStorage.getItem('medicamp_theme') || 'dark';
    } catch (e) {
      savedTheme = 'dark';
    }
    window.setTheme(savedTheme);

    // Restore Language
    let savedLang = 'en';
    try {
      savedLang = localStorage.getItem('medicamp_lang') || 'en';
    } catch (e) {
      savedLang = 'en';
    }
    window.setLanguage(savedLang);

    // Event Listeners for Theme Toggle
    const themeBtn = document.getElementById('themeToggleBtn');
    if (themeBtn) {
      themeBtn.addEventListener('click', function (e) {
        e.preventDefault();
        window.toggleTheme();
      });
    }

    // Event Listeners for Language Toggle
    const btnEn = document.getElementById('btnLangEn');
    const btnBn = document.getElementById('btnLangBn');
    if (btnEn) {
      btnEn.addEventListener('click', function (e) {
        e.preventDefault();
        window.setLanguage('en');
      });
    }
    if (btnBn) {
      btnBn.addEventListener('click', function (e) {
        e.preventDefault();
        window.setLanguage('bn');
      });
    }
  });
})();
