﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Serialization;
using IncomprehensiblePair = System.Collections.Generic.KeyValuePair<string, WordsAndTextsData.WordOrText>;

public partial class Form1 : Form
{
    private FileStream data_file;
    private FormStruct training_form, add_word_form, add_text_form, dictionary_edit_form;
    public WordsAndTextsData Data { get; private set; }
    public LinkedList<IncomprehensiblePair> Incomprehensible { get; private set; }
    public DictionaryChanges DictionaryChangesObserver { get; private set; }

    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        DictionaryChangesObserver = new DictionaryChanges();
        training_form = new TrainingForm(this, TrainingButton, AddIncomprehensible);
        add_word_form = new AddWordsOrTextForms(this, AddWordsButton, CreateData, AddWordsOrTextForms.AddWordsOrTextFormsType.AddWordForm);
        add_text_form = new AddWordsOrTextForms(this, AddTextsButton, CreateData, AddWordsOrTextForms.AddWordsOrTextFormsType.AddTextForm);
        dictionary_edit_form = new DictionaryEditForm(this, DictionaryEditButton);

        DictionaryChangesObserver.Add_AddWordsOrTextForms(add_word_form as AddWordsOrTextForms)
            .Add_AddWordsOrTextForms(add_text_form as AddWordsOrTextForms)
            .Add_DictionaryEditForm(dictionary_edit_form as DictionaryEditForm);

        training_form.BackToMainFormEvent += SetVisibleMainFormElements;
        add_word_form.BackToMainFormEvent += SetVisibleMainFormElements;
        add_text_form.BackToMainFormEvent += SetVisibleMainFormElements;
        dictionary_edit_form.BackToMainFormEvent += SetVisibleMainFormElements;

        try
        {
            data_file = new FileStream("words.data", FileMode.Open);
            if (data_file.Length == 0) throw new FileLoadException();
        }
        catch (FileNotFoundException)
        {
            DictionaryEditButton.Enabled = false;
            TrainingButton.Enabled = false;
            MessageBox.Show("Файл со словарем не найден", "Ошибка");
            return;
        }
        catch (FileLoadException)
        {
            DictionaryEditButton.Enabled = false;
            TrainingButton.Enabled = false;
            MessageBox.Show("Файл со словарем не содержит данных", "Ошибка");
            return;
        }

        Container data = Serialize.DesirializeFromFile(data_file);
        if (data == null)
        {
            DictionaryEditButton.Enabled = false;
            TrainingButton.Enabled = false;
            data_file.Close();
            return;
        }
        Data = new WordsAndTextsData(data.Words, data.Texts);
        Incomprehensible = data.Incomprehensible;
    }

    /*==================== Кнопки ====================*/

    private void TrainingButton_Click(object sender, EventArgs e)
    {
        HideMainFormElements();
    }

    private void AddWordsButton_Click(object sender, EventArgs e)
    {
        HideMainFormElements();
    }

    private void AddTextsButton_Click(object sender, EventArgs e)
    {
        HideMainFormElements();
    }

    private void DictionaryEditButton_Click(object sender, EventArgs e)
    {
        HideMainFormElements();
    }

    /*================================================*/

    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
        if (Data != null)
        {
            if (data_file == null) data_file = File.Create("words.data");
            byte[] buffer = null;
            if (data_file.Length > 0)
            {
                data_file.Seek(0, SeekOrigin.Begin);
                buffer = new byte[data_file.Length];
                data_file.Read(buffer, 0, buffer.Length);
                data_file.SetLength(0);
            }
            try
            {
                Serialize.SerializeToFile(data_file, new Container(Data.Words, Data.Texts, Incomprehensible));
            }
            catch (SerializationException)
            {
                data_file.SetLength(0);
                if (buffer != null) data_file.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                data_file.Close();
            }
        }
    }

    /*==================== Методы ====================*/

    private void SetVisibleMainFormElements()
    {
        TrainingButton.Visible = true;
        AddWordsButton.Visible = true;
        AddTextsButton.Visible = true;
        DictionaryEditButton.Visible = true;
    }

    private void HideMainFormElements()
    {
        TrainingButton.Visible = false;
        AddWordsButton.Visible = false;
        AddTextsButton.Visible = false;
        DictionaryEditButton.Visible = false;
    }

    private void CreateData()
    {
        DictionaryEditButton.Enabled = true;
        TrainingButton.Enabled = true;
        Data = new WordsAndTextsData();
    }

    private void AddIncomprehensible(IncomprehensiblePair pair)
    {
        if (Incomprehensible == null) Incomprehensible = new LinkedList<IncomprehensiblePair>();
        Incomprehensible.AddLast(pair);
    }

    /*================================================*/
}
