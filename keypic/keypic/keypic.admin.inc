<?php

function keypic_admin_form($form, &$form_state) {
  $form['keypic_forms_ids'] = array(
    '#type' => 'textarea',
    '#title' => t('Form CSS IDs'),
    '#rows' => 3,
    '#cols' => 40,
    '#default_value' => variable_get('c_ids', 'contact-site-form'),
    '#description' => t('Enter the CSS IDs of the forms to display captch. One per line.'),
  );
  
  return system_settings_form($form);
}

function keypic_load_include($type, $module, $name = NULL) { 
  if (empty($name)) {
    $name = $module;
  }

  $file = './' . drupal_get_path('module', $module) . "/$name.$type";

  if (is_file($file)) {
    require_once $file;
  }
  else {
    return FALSE;
  }
}

